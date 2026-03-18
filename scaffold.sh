#!/bin/bash

# ==================================================================================
# .NET Entity Scaffold Script v2.0
# ==================================================================================
# Automatically generates Entity, DTOs, Service, Controller, and Validator files
# Supports auto-detection of project namespace and configurable paths
# Inserts service registrations, mapper profiles, and DbContext DbSets
# ==================================================================================

set -e  # Exit on error

# ==================================================================================
# CONFIGURATION
# ==================================================================================

# Detect project root (looks for .csproj file)
detect_project_root() {
    # Prefer the actual ASP.NET Core app directory (Program.cs + appsettings.json)
    if [ -f "Program.cs" ] && [ -f "appsettings.json" ] && ls *.csproj >/dev/null 2>&1; then
        echo "$(pwd)"
        return
    fi

    local app_dir=$(find . -maxdepth 2 -type f -name "Program.cs" -exec dirname {} \; | head -n 1)

    if [ -n "$app_dir" ] && [ -f "$app_dir/appsettings.json" ]; then
        echo "$(cd "$app_dir" && pwd)"
        return
    fi
    
    # Fallback to first non-template-pack project
    local proj_dir=$(find . -maxdepth 2 -name "*.csproj" ! -name "*TemplatePack.csproj" -exec dirname {} \; | head -n 1)
    
    if [ -n "$proj_dir" ]; then
        echo "$(cd "$proj_dir" && pwd)"
        return
    fi
    
    # Fallback to current directory
    echo "$(pwd)"
}

# Default configuration (relative to project root)
CONFIG_ENTITY_PATH="Entities"
CONFIG_DTO_PATH="DTOs"
CONFIG_SERVICE_PATH="Services"
CONFIG_CONTROLLER_PATH="Controllers"
CONFIG_VALIDATOR_PATH="Validators"
CONFIG_PROFILE_PATH="Profiles"

# Files to modify for auto-injection (relative to project root)
CONFIG_DBCONTEXT_FILE="Data/ApplicationDbContext.cs"
CONFIG_MAPPER_FILE="Profiles/MappingProfile.cs"
CONFIG_PROGRAM_FILE="Program.cs"

# ==================================================================================
# UTILITY FUNCTIONS
# ==================================================================================

# Colors for better output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}ℹ${NC}  $1"
}

log_success() {
    echo -e "${GREEN}✅${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}⚠️${NC}  $1"
}

log_error() {
    echo -e "${RED}❌${NC} $1"
}

log_section() {
    echo -e "\n${CYAN}═══ $1 ═══${NC}\n"
}

# Create directory if it doesn't exist
ensure_directory() {
    if [ ! -d "$1" ]; then
        mkdir -p "$1"
        log_info "Created directory: $1"
    fi
}

# Create file with content
create_file() {
    local file_path="$1"
    local content="$2"
    
    ensure_directory "$(dirname "$file_path")"
    echo "$content" > "$file_path"
    log_success "Created: $file_path"
}

# Detect project namespace from .csproj file
detect_namespace() {
    local csproj_file=$(find . -maxdepth 2 -name "*.csproj" | head -n 1)
    
    if [ -z "$csproj_file" ]; then
        log_warning "No .csproj file found. Using default namespace 'YourProjectName'"
        echo "YourProjectName"
        return
    fi
    
    local root_namespace=$(grep -oPm1 '(?<=<RootNamespace>)[^<]+' "$csproj_file" || true)

    if [ -n "$root_namespace" ]; then
        log_info "Detected project namespace from RootNamespace: $root_namespace" >&2
        echo "$root_namespace"
        return
    fi

    local project_name=$(basename "$csproj_file" .csproj)
    log_info "Detected project namespace from project name: $project_name" >&2
    echo "$project_name"
}

# Convert string to camelCase
to_camel_case() {
    local input="$1"
    local first_char="${input:0:1}"
    local rest="${input:1}"
    echo "$(echo "$first_char" | tr '[:upper:]' '[:lower:]')$rest"
}

# Convert string to PascalCase
to_pascal_case() {
    local input="$1"
    local first_char="${input:0:1}"
    local rest="${input:1}"
    echo "$(echo "$first_char" | tr '[:lower:]' '[:upper:]')$rest"
}

# Insert content after a marker in a file
insert_after_marker() {
    local content="$1"
    local file_path="$2"
    local marker="$3"
    
    if [ ! -f "$file_path" ]; then
        log_warning "File not found: $file_path (skipping injection)"
        return 1
    fi
    
    if ! grep -q "$marker" "$file_path"; then
        log_warning "Marker '$marker' not found in $file_path"
        log_info "Please add the marker manually or register services manually"
        return 1
    fi
    
    # Use awk for reliable insertion
    awk -v marker="$marker" -v content="$content" '
        {
            print
            if ($0 ~ marker) {
                print content
            }
        }
    ' "$file_path" > "${file_path}.tmp" && mv "${file_path}.tmp" "$file_path"
    
    log_success "Injected content into: $file_path"
    return 0
}
# Smart injection functions that find the right place automatically

# Inject DbSet into DbContext
inject_dbset() {
    local entity_name="$1"
    local file_path="$2"
    local entity_namespace="$3"
    local entity_type="$entity_name"

    if [ -n "$entity_namespace" ]; then
        entity_type="${entity_namespace}.${entity_name}"
    fi
    
    if [ ! -f "$file_path" ]; then
        log_warning "DbContext file not found: $file_path"
        log_info "Add this line manually to your DbContext:"
        echo "    public DbSet<${entity_name}> ${entity_name}s { get; set; }"
        return 1
    fi
    
    local dbset_line="    public DbSet<${entity_type}> ${entity_name}s { get; set; }"
    
    # Check if DbSet already exists
    if grep -q "DbSet<${entity_name}>" "$file_path"; then
        log_warning "DbSet for ${entity_name} already exists in DbContext"
        return 0
    fi
    
    # Find the last DbSet and insert after it, or before the closing brace
    if grep -q "public DbSet<" "$file_path"; then
        # Insert after the last DbSet
        awk -v line="$dbset_line" '
            /public DbSet</ { last = NR }
            { lines[NR] = $0 }
            END {
                for (i = 1; i <= NR; i++) {
                    print lines[i]
                    if (i == last) print line
                }
            }
        ' "$file_path" > "${file_path}.tmp" && mv "${file_path}.tmp" "$file_path"
    else
        # Insert before the last closing brace
        awk -v line="$dbset_line" '
            { lines[++count] = $0 }
            END {
                for (i = 1; i < count; i++) print lines[i]
                print line
                print lines[count]
            }
        ' "$file_path" > "${file_path}.tmp" && mv "${file_path}.tmp" "$file_path"
    fi
    
    log_success "Added DbSet to: $file_path"
    return 0
}

# Inject AutoMapper profiles
inject_mapper_profile() {
    local entity_name="$1"
    local file_path="$2"
    local namespace="$3"
    local entity_namespace="$4"
    
    if [ ! -f "$file_path" ]; then
        log_warning "Mapper profile file not found: $file_path"
        log_info "Add these mappings manually to your MappingProfile:"
        echo "    CreateMap<${entity_namespace}.${entity_name}, ${namespace}.DTOs.${entity_name}Dto>();"
        echo "    CreateMap<${namespace}.DTOs.${entity_name}Form, ${entity_namespace}.${entity_name}>();"
        echo "    CreateMap<${namespace}.DTOs.${entity_name}Update, ${entity_namespace}.${entity_name}>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));"
        return 1
    fi
    
    # Check if mapping already exists
    if grep -q "CreateMap<${entity_name}," "$file_path"; then
        log_warning "Mapping for ${entity_name} already exists"
        return 0
    fi
    
    local mapping_content="        CreateMap<${entity_namespace}.${entity_name}, ${namespace}.DTOs.${entity_name}Dto>();
        CreateMap<${namespace}.DTOs.${entity_name}Form, ${entity_namespace}.${entity_name}>();
        CreateMap<${namespace}.DTOs.${entity_name}Update, ${entity_namespace}.${entity_name}>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));"
    
    # Find constructor and insert mappings after the opening brace or before closing brace
    if grep -q "public.*MappingProfile" "$file_path"; then
        # Insert after the constructor opening brace
        awk -v content="$mapping_content" '
            /public.*MappingProfile.*\{/ { print; print content; next }
            /CreateMap</ { last_map = NR }
            { lines[NR] = $0 }
            END {
                if (last_map > 0) {
                    for (i = 1; i <= NR; i++) {
                        if (i != 1 || lines[1] !~ /public.*MappingProfile.*\{/) print lines[i]
                        if (i == last_map) print content
                    }
                } else {
                    for (i = 1; i <= NR; i++) {
                        if (i != 1 || lines[1] !~ /public.*MappingProfile.*\{/) print lines[i]
                    }
                }
            }
        ' "$file_path" > "${file_path}.tmp" && mv "${file_path}.tmp" "$file_path"
    else
        log_warning "Could not find MappingProfile constructor"
        return 1
    fi
    
    log_success "Added mappings to: $file_path"
    return 0
}

# Inject service registration into Program.cs
inject_service_registration() {
    local entity_name="$1"
    local file_path="$2"
    local namespace="$3"
    
    if [ ! -f "$file_path" ]; then
        log_warning "Program.cs not found: $file_path"
        log_info "Add this line manually to Program.cs:"
        echo "builder.Services.AddScoped<${namespace}.Services.${entity_name}.I${entity_name}Service, ${namespace}.Services.${entity_name}.${entity_name}Service>();"
        return 1
    fi
    
    # Check if service already registered
    if grep -q "I${entity_name}Service" "$file_path"; then
        log_warning "Service ${entity_name}Service already registered"
        return 0
    fi
    
    local service_line="builder.Services.AddScoped<${namespace}.Services.${entity_name}.I${entity_name}Service, ${namespace}.Services.${entity_name}.${entity_name}Service>();"
    
    # Find where other services are registered (look for AddScoped, AddTransient, AddSingleton)
    if grep -q "builder.Services.AddScoped<I.*Service," "$file_path"; then
        # Insert after the last AddScoped for a service
        awk -v line="$service_line" '
            /builder\.Services\.AddScoped<I.*Service,/ { last = NR }
            { lines[NR] = $0 }
            END {
                for (i = 1; i <= NR; i++) {
                    print lines[i]
                    if (i == last) print line
                }
            }
        ' "$file_path" > "${file_path}.tmp" && mv "${file_path}.tmp" "$file_path"
    elif grep -q "builder.Services.AddCors" "$file_path"; then
        # Insert before CORS registration to avoid injecting inside AddDbContext lambdas
        awk -v line="$service_line" '
            /builder\.Services\.AddCors/ && !inserted { print line; inserted = 1 }
            { print }
        ' "$file_path" > "${file_path}.tmp" && mv "${file_path}.tmp" "$file_path"
    elif grep -q "var app = builder.Build\(\);" "$file_path"; then
        # Fallback: keep DI registrations above app build
        awk -v line="$service_line" '
            /var app = builder\.Build\(\);/ && !inserted { print line; inserted = 1 }
            { print }
        ' "$file_path" > "${file_path}.tmp" && mv "${file_path}.tmp" "$file_path"
    else
        log_warning "Could not find suitable injection point in Program.cs"
        log_info "Add this line manually after your service registrations:"
        echo "$service_line"
        return 1
    fi
    
    log_success "Added service registration to: $file_path"
    return 0
}

# Validate entity name
validate_entity_name() {
    local name="$1"
    
    if [ -z "$name" ]; then
        log_error "Entity name cannot be empty"
        return 1
    fi
    
    if ! [[ "$name" =~ ^[A-Za-z][A-Za-z0-9]*$ ]]; then
        log_error "Invalid entity name. Use only letters and numbers, starting with a letter."
        return 1
    fi
    
    return 0
}

# ==================================================================================
# MAIN SCAFFOLDING LOGIC
# ==================================================================================

scaffold_entity() {
    log_section "Entity Scaffold Generator"
    
    # Detect and navigate to project root
    PROJECT_ROOT=$(detect_project_root)
    
    if [ ! -d "$PROJECT_ROOT" ]; then
        log_error "Could not detect project root directory"
        exit 1
    fi
    
    log_info "Project root: $PROJECT_ROOT"
    
    # Change to project root
    cd "$PROJECT_ROOT" || exit 1
    
    # Detect namespace
    NAMESPACE=$(detect_namespace)
    
    # Get entity name
    read -p "Enter Entity name (PascalCase): " raw_name
    
    if ! validate_entity_name "$raw_name"; then
        exit 1
    fi
    
    ENTITY_NAME=$(to_pascal_case "$raw_name")
    ENTITY_NAME_LOWER=$(to_camel_case "$ENTITY_NAME")
    
    log_info "Entity name: $ENTITY_NAME"
    
    # ID Type selection
    while true; do
        read -p "Choose ID type (1=int, 2=Guid, 3=string): " id_choice
        case $id_choice in
            1) ID_TYPE="int"; break ;;
            2) ID_TYPE="Guid"; break ;;
            3) ID_TYPE="string"; break ;;
            *) log_warning "Please enter 1, 2, or 3." ;;
        esac
    done
    
    log_info "ID type: $ID_TYPE"
    
    # Authorization requirement
    while true; do
        read -p "Require authorization on controller? (y/n): " auth_choice
        case $auth_choice in
            [Yy]*) REQUIRE_AUTH="[Authorize]"; break ;;
            [Nn]*) REQUIRE_AUTH=""; break ;;
            *) log_warning "Please answer y or n." ;;
        esac
    done
    
    # Use subdirectory for DTOs?
    while true; do
        read -p "Create DTOs in entity subfolder? (y/n): " dto_subfolder
        case $dto_subfolder in
            [Yy]*) DTO_SUBFOLDER="$ENTITY_NAME"; break ;;
            [Nn]*) DTO_SUBFOLDER=""; break ;;
            *) log_warning "Please answer y or n." ;;
        esac
    done
    
    # Use subdirectory for Entities?
    while true; do
        read -p "Create Entity in subfolder? (e.g., Applications, Users, etc. or press Enter for root): " entity_subfolder
        ENTITY_SUBFOLDER="$entity_subfolder"
        break
    done
    
    # Generate validators?
    while true; do
        read -p "Generate FluentValidation validators? (y/n): " gen_validators
        case $gen_validators in
            [Yy]*) GENERATE_VALIDATORS=true; break ;;
            [Nn]*) GENERATE_VALIDATORS=false; break ;;
            *) log_warning "Please answer y or n." ;;
        esac
    done
    
    # Define paths (all relative to project root)
    if [ -n "$ENTITY_SUBFOLDER" ]; then
        ENTITY_FILE_PATH="$CONFIG_ENTITY_PATH/$ENTITY_SUBFOLDER/$ENTITY_NAME.cs"
    else
        ENTITY_FILE_PATH="$CONFIG_ENTITY_PATH/$ENTITY_NAME.cs"
    fi
    
    if [ -n "$DTO_SUBFOLDER" ]; then
        DTO_PATH="$CONFIG_DTO_PATH/$DTO_SUBFOLDER"
    else
        DTO_PATH="$CONFIG_DTO_PATH"
    fi
    
    SERVICE_FILE_PATH="$CONFIG_SERVICE_PATH/${ENTITY_NAME}/${ENTITY_NAME}Service.cs"
    CONTROLLER_FILE_PATH="$CONFIG_CONTROLLER_PATH/${ENTITY_NAME}/${ENTITY_NAME}Controller.cs"
    
    # Check if entity already exists
    if [ -f "$ENTITY_FILE_PATH" ]; then
        log_error "Entity '$ENTITY_NAME' already exists at $ENTITY_FILE_PATH"
        exit 1
    fi
    
    log_section "Generating Files"
    
    # ==================================================================================
    # GENERATE ENTITY
    # ==================================================================================
    
    ENTITY_NAMESPACE="${NAMESPACE}.Entities"
    if [ -n "$ENTITY_SUBFOLDER" ]; then
        ENTITY_NAMESPACE="${NAMESPACE}.Entities.${ENTITY_SUBFOLDER}"
    fi
    
    ENTITY_CONTENT="namespace ${ENTITY_NAMESPACE};

public class ${ENTITY_NAME}
{
    public ${ID_TYPE} Id { get; set; }
    
    // TODO: Add your properties here
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}"
    
    create_file "$ENTITY_FILE_PATH" "$ENTITY_CONTENT"
    
    # ==================================================================================
    # GENERATE DTOs
    # ==================================================================================
    
    DTO_CONTENT="namespace ${NAMESPACE}.DTOs;

public class ${ENTITY_NAME}Dto
{
    public ${ID_TYPE} Id { get; set; }
    
    // TODO: Add your DTO properties here
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}"
    
    FORM_CONTENT="namespace ${NAMESPACE}.DTOs;

public class ${ENTITY_NAME}Form
{
    // TODO: Add properties required for creating ${ENTITY_NAME}
}"
    
    UPDATE_CONTENT="namespace ${NAMESPACE}.DTOs;

public class ${ENTITY_NAME}Update
{
    // TODO: Add properties for updating ${ENTITY_NAME}
    // Use nullable types for optional updates
}"
    
    FILTER_CONTENT="namespace ${NAMESPACE}.DTOs;

public class ${ENTITY_NAME}Filter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // TODO: Add filtering properties
}"
    
    create_file "$DTO_PATH/${ENTITY_NAME}Dto.cs" "$DTO_CONTENT"
    create_file "$DTO_PATH/${ENTITY_NAME}Form.cs" "$FORM_CONTENT"
    create_file "$DTO_PATH/${ENTITY_NAME}Update.cs" "$UPDATE_CONTENT"
    create_file "$DTO_PATH/${ENTITY_NAME}Filter.cs" "$FILTER_CONTENT"
    
    # ==================================================================================
    # GENERATE SERVICE
    # ==================================================================================
    
    SERVICE_CONTENT="using ${NAMESPACE}.DTOs;
using ${ENTITY_NAMESPACE};
using ${NAMESPACE}.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ${NAMESPACE}.Services.${ENTITY_NAME};

public interface I${ENTITY_NAME}Service
{
    Task<${ENTITY_NAME}Dto?> GetByIdAsync(${ID_TYPE} id);
    Task<(List<${ENTITY_NAME}Dto> items, int totalCount)> GetAllAsync(${ENTITY_NAME}Filter filter);
    Task<${ENTITY_NAME}Dto> CreateAsync(${ENTITY_NAME}Form form);
    Task<${ENTITY_NAME}Dto?> UpdateAsync(${ID_TYPE} id, ${ENTITY_NAME}Update update);
    Task<bool> DeleteAsync(${ID_TYPE} id);
}

public class ${ENTITY_NAME}Service : I${ENTITY_NAME}Service
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ${ENTITY_NAME}Service(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<${ENTITY_NAME}Dto?> GetByIdAsync(${ID_TYPE} id)
    {
        var entity = await _context.${ENTITY_NAME}s.FindAsync(id);
        return entity == null ? null : _mapper.Map<${ENTITY_NAME}Dto>(entity);
    }

    public async Task<(List<${ENTITY_NAME}Dto> items, int totalCount)> GetAllAsync(${ENTITY_NAME}Filter filter)
    {
        var query = _context.${ENTITY_NAME}s.AsQueryable();
        
        // TODO: Apply filters
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        
        return (_mapper.Map<List<${ENTITY_NAME}Dto>>(items), totalCount);
    }

    public async Task<${ENTITY_NAME}Dto> CreateAsync(${ENTITY_NAME}Form form)
    {
        var entity = _mapper.Map<${ENTITY_NAMESPACE}.${ENTITY_NAME}>(form);
        
        _context.${ENTITY_NAME}s.Add(entity);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<${ENTITY_NAME}Dto>(entity);
    }

    public async Task<${ENTITY_NAME}Dto?> UpdateAsync(${ID_TYPE} id, ${ENTITY_NAME}Update update)
    {
        var entity = await _context.${ENTITY_NAME}s.FindAsync(id);
        
        if (entity == null)
            return null;
        
        _mapper.Map(update, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return _mapper.Map<${ENTITY_NAME}Dto>(entity);
    }

    public async Task<bool> DeleteAsync(${ID_TYPE} id)
    {
        var entity = await _context.${ENTITY_NAME}s.FindAsync(id);
        
        if (entity == null)
            return false;
        
        _context.${ENTITY_NAME}s.Remove(entity);
        await _context.SaveChangesAsync();
        
        return true;
    }
}"
    
    create_file "$SERVICE_FILE_PATH" "$SERVICE_CONTENT"
    
    # ==================================================================================
    # GENERATE CONTROLLER
    # ==================================================================================
    
    AUTH_ATTRIBUTE=""
    if [ -n "$REQUIRE_AUTH" ]; then
        AUTH_ATTRIBUTE="
    $REQUIRE_AUTH"
    fi
    
    CONTROLLER_CONTENT="using ${NAMESPACE}.DTOs;
using ${NAMESPACE}.Services.${ENTITY_NAME};
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ${NAMESPACE}.Controllers.${ENTITY_NAME};

[ApiController]
[Route(\"api/[controller]\")]${AUTH_ATTRIBUTE}
public class ${ENTITY_NAME}Controller : ControllerBase
{
    private readonly I${ENTITY_NAME}Service _${ENTITY_NAME_LOWER}Service;

    public ${ENTITY_NAME}Controller(I${ENTITY_NAME}Service ${ENTITY_NAME_LOWER}Service)
    {
        _${ENTITY_NAME_LOWER}Service = ${ENTITY_NAME_LOWER}Service;
    }

    [HttpGet(\"{id}\")]
    public async Task<ActionResult<${ENTITY_NAME}Dto>> GetById(${ID_TYPE} id)
    {
        var result = await _${ENTITY_NAME_LOWER}Service.GetByIdAsync(id);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<${ENTITY_NAME}Dto>>> GetAll([FromQuery] ${ENTITY_NAME}Filter filter)
    {
        var (items, totalCount) = await _${ENTITY_NAME_LOWER}Service.GetAllAsync(filter);
        
        Response.Headers.Append(\"X-Total-Count\", totalCount.ToString());
        
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<${ENTITY_NAME}Dto>> Create([FromBody] ${ENTITY_NAME}Form form)
    {
        var result = await _${ENTITY_NAME_LOWER}Service.CreateAsync(form);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut(\"{id}\")]
    public async Task<ActionResult<${ENTITY_NAME}Dto>> Update(${ID_TYPE} id, [FromBody] ${ENTITY_NAME}Update update)
    {
        var result = await _${ENTITY_NAME_LOWER}Service.UpdateAsync(id, update);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    [HttpDelete(\"{id}\")]
    public async Task<ActionResult> Delete(${ID_TYPE} id)
    {
        var success = await _${ENTITY_NAME_LOWER}Service.DeleteAsync(id);
        
        if (!success)
            return NotFound();
        
        return NoContent();
    }
}"
    
    create_file "$CONTROLLER_FILE_PATH" "$CONTROLLER_CONTENT"
    
    # ==================================================================================
    # GENERATE VALIDATORS (if requested)
    # ==================================================================================
    
    if [ "$GENERATE_VALIDATORS" = true ]; then
        VALIDATOR_PATH="$CONFIG_VALIDATOR_PATH/$ENTITY_NAME"
        
        FORM_VALIDATOR="using ${NAMESPACE}.DTOs;
using FluentValidation;

namespace ${NAMESPACE}.Validators.${ENTITY_NAME};

public class ${ENTITY_NAME}FormValidator : AbstractValidator<${ENTITY_NAME}Form>
{
    public ${ENTITY_NAME}FormValidator()
    {
        // TODO: Add validation rules
        // Example: RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}"
        
        UPDATE_VALIDATOR="using ${NAMESPACE}.DTOs;
using FluentValidation;

namespace ${NAMESPACE}.Validators.${ENTITY_NAME};

public class ${ENTITY_NAME}UpdateValidator : AbstractValidator<${ENTITY_NAME}Update>
{
    public ${ENTITY_NAME}UpdateValidator()
    {
        // TODO: Add validation rules
    }
}"
        
        FILTER_VALIDATOR="using ${NAMESPACE}.DTOs;
using FluentValidation;

namespace ${NAMESPACE}.Validators.${ENTITY_NAME};

public class ${ENTITY_NAME}FilterValidator : AbstractValidator<${ENTITY_NAME}Filter>
{
    public ${ENTITY_NAME}FilterValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        // TODO: Add more validation rules
    }
}"
        
        create_file "$VALIDATOR_PATH/${ENTITY_NAME}FormValidator.cs" "$FORM_VALIDATOR"
        create_file "$VALIDATOR_PATH/${ENTITY_NAME}UpdateValidator.cs" "$UPDATE_VALIDATOR"
        create_file "$VALIDATOR_PATH/${ENTITY_NAME}FilterValidator.cs" "$FILTER_VALIDATOR"
    fi
    
    # ==================================================================================
    # AUTO-INJECTION
    # ==================================================================================
    
    log_section "Auto-Injection"
    
    # Inject DbSet
    inject_dbset "$ENTITY_NAME" "$CONFIG_DBCONTEXT_FILE" "$ENTITY_NAMESPACE"
    
    # Inject Mapper Profiles
    inject_mapper_profile "$ENTITY_NAME" "$CONFIG_MAPPER_FILE" "$NAMESPACE" "$ENTITY_NAMESPACE"
    
    # Inject Service Registration
    inject_service_registration "$ENTITY_NAME" "$CONFIG_PROGRAM_FILE" "$NAMESPACE"
    
    # ==================================================================================
    # SUMMARY
    # ==================================================================================
    
    log_section "Scaffold Summary"
    
    echo "Entity: $ENTITY_NAME"
    echo "ID Type: $ID_TYPE"
    echo "Authorization: $([ -n "$REQUIRE_AUTH" ] && echo "Required" || echo "Not Required")"
    echo ""
    echo "Generated Files:"
    echo "  • $ENTITY_FILE_PATH"
    echo "  • $DTO_PATH/${ENTITY_NAME}Dto.cs"
    echo "  • $DTO_PATH/${ENTITY_NAME}Form.cs"
    echo "  • $DTO_PATH/${ENTITY_NAME}Update.cs"
    echo "  • $DTO_PATH/${ENTITY_NAME}Filter.cs"
    echo "  • $SERVICE_FILE_PATH"
    echo "  • $CONTROLLER_FILE_PATH"
    
    if [ "$GENERATE_VALIDATORS" = true ]; then
        echo "  • $VALIDATOR_PATH/${ENTITY_NAME}FormValidator.cs"
        echo "  • $VALIDATOR_PATH/${ENTITY_NAME}UpdateValidator.cs"
        echo "  • $VALIDATOR_PATH/${ENTITY_NAME}FilterValidator.cs"
    fi
    
    echo ""
    log_success "🎉 Scaffold completed successfully!"
    echo ""
    log_info "Next steps:"
    echo "  1. Add properties to your Entity and DTOs"
    echo "  2. Run: dotnet ef migrations add Add${ENTITY_NAME}"
    echo "  3. Run: dotnet ef database update"
    
    if [ "$GENERATE_VALIDATORS" = true ]; then
        echo "  4. Implement validation rules in validators"
    fi
}

# ==================================================================================
# ENTRY POINT
# ==================================================================================

main() {
    scaffold_entity
}

main "$@"
