# Hướng Dẫn Mở Rộng Workflow System

## 📖 Tổng Quan

Hệ thống workflow đã được thiết kế với khả năng mở rộng cao thông qua **Strategy Pattern** và **Validation Pipeline**. Bạn có thể dễ dàng thêm:

1. **Các loại approver mới** (custom approver strategies)
2. **Các quy tắc validation mới** (custom validators)
3. **Không cần sửa đổi code cốt lõi**

---

## 🎯 Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    WorkflowService                      │
│                                                         │
│  Uses:                                                  │
│  - ApproverStrategyFactory (extensible)                │
│  - ValidationPipeline (composable)                     │
└─────────────────────────────────────────────────────────┘
                    ↓                  ↓
        ┌───────────────────┐  ┌──────────────────┐
        │ Approver Strategy │  │    Validators    │
        │     Factory       │  │     Pipeline     │
        └───────────────────┘  └──────────────────┘
                ↓                       ↓
    ┌───────────────────────┐  ┌──────────────────────┐
    │  Built-in Strategies: │  │  Built-in Validators:│
    │  - Users              │  │  - FileType          │
    │  - Department         │  │  - Permission        │
    │  + Custom strategies  │  │  - ApproverScope     │
    └───────────────────────┘  │  + Custom validators │
                               └──────────────────────┘
```

---

## 📝 Part 1: Adding Custom Approver Strategies

### Step 1: Implement IApproverStrategy

```csharp
using Workflow.Services.Strategies;
using Workflow.Models;

public class MyCustomApproverStrategy : IApproverStrategy
{
    public string ApproverType => "MyCustomType";

    public async Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
    {
        // Your logic here
        // Example: Check if user has specific role, is in hierarchy, etc.
        return true; // or false based on your logic
    }

    public async Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
    {
        // Return list of all users who can approve at this level
        // Used for notifications
        return new List<Guid>();
    }
}
```

### Step 2: Register Strategy in Program.cs

```csharp
// Add to service registration section
builder.Services.AddSingleton<IApproverStrategy, MyCustomApproverStrategy>();
```

### Step 3: Use in Template Configuration

```json
{
  "name": "My Workflow Template",
  "levels": [
    {
      "order": 1,
      "approverType": "MyCustomType",  // Your custom type
      "requiredApprovals": 1,
      // Add custom fields as needed
      "customField": "customValue"
    }
  ]
}
```

---

## 🔍 Built-in Strategy Examples

### 1. Role-Based Approver Strategy

**Scenario**: Approve based on user roles (e.g., "Manager", "Director")

**Implementation**: See `Services/Strategies/Examples/RoleBasedApproverStrategy.cs`

```csharp
// Register in Program.cs
builder.Services.AddSingleton<IApproverStrategy, RoleBasedApproverStrategy>();

// Use in template
{
  "approverType": "Role",
  "roleNamesJson": "[\"Manager\", \"Director\"]"
}
```

### 2. Hierarchy-Based Strategy

**Scenario**: Approve based on organizational hierarchy (direct manager, skip-level manager)

**Implementation**: See `Services/Strategies/Examples/HierarchyApproverStrategy.cs`

```csharp
// Register
builder.Services.AddSingleton<IApproverStrategy, HierarchyApproverStrategy>();

// Use
{
  "approverType": "DirectManager",
  "hierarchyLevel": 1  // 1 = direct manager, 2 = manager's manager
}
```

### 3. Conditional Strategy

**Scenario**: Different approvers based on document properties (amount, type, etc.)

**Implementation**: See `Services/Strategies/Examples/ConditionalApproverStrategy.cs`

```csharp
// Register
builder.Services.AddSingleton<IApproverStrategy, ConditionalApproverStrategy>();

// Use
{
  "approverType": "Conditional",
  "conditionsJson": "[{\"field\": \"amount\", \"operator\": \"<\", \"value\": 1000, \"approvers\": [...]}]"
}
```

---

## ✅ Part 2: Adding Custom Validators

### Step 1: Implement IWorkflowValidator

```csharp
using Workflow.Services.Validators;

public class MyCustomValidator : IWorkflowValidator
{
    public string Name => "MyValidator";

    public async Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
    {
        // Access:
        // - context.Instance (workflow instance)
        // - context.CurrentLevel (current approval level)
        // - context.Document (document being approved)
        // - context.ApproverId (user trying to approve)

        if (/* some condition fails */)
        {
            return ValidationResult.Failure("Error message here");
        }

        return ValidationResult.Success();
    }
}
```

### Step 2: Register Validator in Program.cs

```csharp
// Add to validation pipeline
builder.Services.AddScoped<ValidationPipeline>(sp =>
{
    var pipeline = new ValidationPipeline();
    
    // Built-in validators
    pipeline.AddValidator(new FileTypeValidator());
    pipeline.AddValidator(new UserFileTypePermissionValidator(...));
    pipeline.AddValidator(new ApproverScopeValidator(...));
    
    // Add your custom validator
    pipeline.AddValidator(new MyCustomValidator());
    
    return pipeline;
});
```

**Note**: Validators run in the order they are added. First failure stops the pipeline.

---

## 🔧 Built-in Validator Examples

### 1. Document Size Validator

**Purpose**: Restrict file sizes at different approval levels

**Implementation**: See `Services/Validators/Examples/DocumentSizeValidator.cs`

```csharp
// Usage
pipeline.AddValidator(new DocumentSizeValidator());

// Configuration in template
{
  "maxFileSizeBytes": 5242880  // 5MB
}
```

### 2. Business Hours Validator

**Purpose**: Only allow approvals during business hours

**Implementation**: See `Services/Validators/Examples/BusinessHoursValidator.cs`

```csharp
// Usage
pipeline.AddValidator(new BusinessHoursValidator());
// No additional configuration needed - uses predefined hours
```

### 3. Duplicate Approval Validator

**Purpose**: Prevent same user from approving multiple levels (separation of duties)

**Implementation**: See `Services/Validators/Examples/DuplicateApprovalValidator.cs`

```csharp
// Usage
pipeline.AddValidator(new DuplicateApprovalValidator());
// Automatically checks approval history
```

### 4. Amount Threshold Validator

**Purpose**: Validate approval authority based on document amount

**Implementation**: See `Services/Validators/Examples/AmountThresholdValidator.cs`

```csharp
// Usage
pipeline.AddValidator(new AmountThresholdValidator());
// Requires Amount field on Document model
```

---

## 🚀 Real-World Extension Scenarios

### Scenario 1: Add "Team Lead" Approver Type

**Requirement**: Team leads can approve documents from their team members.

**Steps**:

1. Create `TeamLeadApproverStrategy.cs`:
```csharp
public class TeamLeadApproverStrategy : IApproverStrategy
{
    private readonly WorkflowDbContext _context;
    
    public string ApproverType => "TeamLead";
    
    public async Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
    {
        // Check if user is team lead and instance requester is in their team
        var user = await _context.Users.Include(u => u.Team).FirstOrDefaultAsync(u => u.Id == userId);
        return user?.IsTeamLead == true;
    }
    
    // ... implement GetApproverUserIdsAsync
}
```

2. Register in `Program.cs`:
```csharp
builder.Services.AddSingleton<IApproverStrategy, TeamLeadApproverStrategy>();
```

3. Use in templates:
```json
{
  "approverType": "TeamLead",
  "requiredApprovals": 1
}
```

### Scenario 2: Add Time-Based Validation

**Requirement**: Urgent requests can be approved 24/7, normal requests only during business hours.

**Steps**:

1. Create `TimeBasedValidator.cs`:
```csharp
public class TimeBasedValidator : IWorkflowValidator
{
    public string Name => "TimeBased";
    
    public Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
    {
        // Check if document is marked as urgent
        if (context.Document.IsUrgent)
            return Task.FromResult(ValidationResult.Success());
            
        // For non-urgent, check business hours
        var now = DateTime.Now;
        if (now.Hour < 8 || now.Hour > 18)
            return Task.FromResult(ValidationResult.Failure("Non-urgent approvals only during 8AM-6PM"));
            
        return Task.FromResult(ValidationResult.Success());
    }
}
```

2. Register in pipeline.

---

## 🔄 Dynamic Strategy Registration

You can also register strategies dynamically:

```csharp
// In Program.cs or a configuration service
var strategyFactory = app.Services.GetRequiredService<ApproverStrategyFactory>();

// Register custom strategy at runtime
strategyFactory.RegisterStrategy(new CustomApproverStrategy());
```

---

## 📊 Validation Pipeline Composition

Create different pipelines for different workflows:

```csharp
// Standard workflow
var standardPipeline = new ValidationPipeline()
    .AddValidator(new FileTypeValidator())
    .AddValidator(new UserFileTypePermissionValidator(...))
    .AddValidator(new ApproverScopeValidator(...));

// Financial workflow (stricter)
var financialPipeline = new ValidationPipeline()
    .AddValidator(new FileTypeValidator())
    .AddValidator(new UserFileTypePermissionValidator(...))
    .AddValidator(new ApproverScopeValidator(...))
    .AddValidator(new AmountThresholdValidator())
    .AddValidator(new DuplicateApprovalValidator())
    .AddValidator(new BusinessHoursValidator());
```

---

## 🧪 Testing Custom Strategies and Validators

### Testing a Strategy

```csharp
[Fact]
public async Task MyStrategy_UserInScope_ReturnsTrue()
{
    var strategy = new MyCustomApproverStrategy();
    var level = new WorkflowLevel { /* setup */ };
    
    var result = await strategy.IsUserInScopeAsync(userId, level);
    
    Assert.True(result);
}
```

### Testing a Validator

```csharp
[Fact]
public async Task MyValidator_InvalidCondition_ReturnsFailure()
{
    var validator = new MyCustomValidator();
    var context = new WorkflowValidationContext { /* setup */ };
    
    var result = await validator.ValidateAsync(context);
    
    Assert.False(result.IsValid);
    Assert.NotNull(result.ErrorMessage);
}
```

---

## 📚 Best Practices

### ✅ DO:
- Keep strategies focused on a single approver type
- Keep validators focused on a single validation rule
- Document the purpose and configuration of custom components
- Handle null/missing data gracefully
- Use async/await properly
- Add logging for debugging

### ❌ DON'T:
- Mix multiple concerns in one strategy/validator
- Throw exceptions from validators (return ValidationResult.Failure instead)
- Hardcode configuration (use database or config files)
- Forget to register new components in Program.cs
- Create circular dependencies between strategies

---

## 🔍 Debugging Tips

### Enable Detailed Logging

```csharp
// In Program.cs
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});

// In your strategy/validator
private readonly ILogger<MyStrategy> _logger;

public async Task<bool> IsUserInScopeAsync(...)
{
    _logger.LogDebug("Checking user {UserId} for level {LevelId}", userId, level.Id);
    // ... logic
}
```

### View Registered Strategies

```csharp
// Add API endpoint to list strategies
[HttpGet("api/workflow/strategies")]
public IActionResult GetStrategies([FromServices] ApproverStrategyFactory factory)
{
    return Ok(factory.GetSupportedApproverTypes());
}
```

### View Validation Pipeline

```csharp
// Add API endpoint to list validators
[HttpGet("api/workflow/validators")]
public IActionResult GetValidators([FromServices] ValidationPipeline pipeline)
{
    return Ok(pipeline.GetValidatorNames());
}
```

---

## 🎓 Advanced Topics

### Multi-Tenancy Support

Create tenant-specific strategies:

```csharp
public class TenantAwareApproverStrategy : IApproverStrategy
{
    private readonly ITenantProvider _tenantProvider;
    
    public async Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
    {
        var tenantId = _tenantProvider.GetCurrentTenant();
        // Use tenant-specific logic
    }
}
```

### Caching Strategy Results

```csharp
public class CachedApproverStrategy : IApproverStrategy
{
    private readonly IApproverStrategy _inner;
    private readonly IMemoryCache _cache;
    
    public async Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
    {
        var cacheKey = $"approvers_{level.Id}";
        
        if (_cache.TryGetValue(cacheKey, out List<Guid> cached))
            return cached;
            
        var result = await _inner.GetApproverUserIdsAsync(level);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }
}
```

### Async Validators with External Services

```csharp
public class ExternalApiValidator : IWorkflowValidator
{
    private readonly HttpClient _httpClient;
    
    public async Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
    {
        // Call external API
        var response = await _httpClient.GetAsync($"api/validate/{context.Document.Id}");
        
        if (!response.IsSuccessStatusCode)
            return ValidationResult.Failure("External validation failed");
            
        return ValidationResult.Success();
    }
}
```

---

## 📞 Getting Help

- Check examples in `Services/Strategies/Examples/` and `Services/Validators/Examples/`
- Review existing built-in strategies and validators
- Check unit tests (when available)
- Read the main documentation: `README_VN.md`

---

## 🎉 Summary

Hệ thống workflow có thể mở rộng dễ dàng thông qua:

1. **IApproverStrategy**: Thêm các loại approver mới
2. **IWorkflowValidator**: Thêm các quy tắc validation mới
3. **ApproverStrategyFactory**: Quản lý tập trung các strategies
4. **ValidationPipeline**: Kết hợp linh hoạt các validators

**Không cần sửa đổi code cốt lõi** - chỉ cần implement interface và register trong `Program.cs`!

---

*Chúc bạn thành công với việc mở rộng workflow system!* 🚀
