# Migration Guide - Upgrade to Extensible Architecture

## 📋 Overview

This guide helps you understand how the new extensible architecture works and how it maintains backward compatibility with existing code.

## ✅ No Action Required for Existing Workflows

**Good news**: All existing workflows continue to work without any changes!

- ✅ Existing templates work unchanged
- ✅ All API endpoints remain the same
- ✅ Database schema unchanged
- ✅ Existing workflows in progress continue normally

## 🔍 What Changed

### Internal Architecture (Not Visible to Users)

The following internal changes improve code quality but don't affect functionality:

1. **WorkflowService Dependencies**
   - **Before**: Used `IFileTypeService` and `IPermissionService` directly
   - **After**: Uses `ApproverStrategyFactory` and `ValidationPipeline`
   - **Impact**: None - same behavior, better code organization

2. **Validation Logic**
   - **Before**: Hard-coded in WorkflowService methods
   - **After**: Extracted into separate validator classes
   - **Impact**: None - same validation rules apply

3. **Approver Resolution**
   - **Before**: if/else statements in WorkflowService
   - **After**: Strategy pattern with dedicated classes
   - **Impact**: None - "Users" and "Department" types work identically

## 📊 Comparison

### Approval Flow (No Change)

**Before**:
```
User requests approval → WorkflowService validates → Approval created
                         (inline validation code)
```

**After**:
```
User requests approval → WorkflowService validates → Approval created
                         (delegated to validators)
```

Result: **Same behavior, cleaner code**

### Approver Type "Users" (No Change)

**Before**: Hard-coded in `IsUserInLevelScopeAsync()`
**After**: `UsersApproverStrategy` class
**Behavior**: Identical - checks if userId is in UserIdsJson

### Approver Type "Department" (No Change)

**Before**: Hard-coded in `IsUserInLevelScopeAsync()`
**After**: `DepartmentApproverStrategy` class
**Behavior**: Identical - returns false (not fully implemented)

## 🆕 New Capabilities (Optional)

If you want to extend the system, you now have options:

### Option 1: Add Custom Approver Type

```csharp
// New file: Services/Strategies/TeamLeadApproverStrategy.cs
public class TeamLeadApproverStrategy : IApproverStrategy
{
    public string ApproverType => "TeamLead";
    
    public async Task<bool> IsUserInScopeAsync(Guid userId, WorkflowLevel level)
    {
        // Your custom logic
        return true;
    }
    
    public async Task<List<Guid>> GetApproverUserIdsAsync(WorkflowLevel level)
    {
        // Your custom logic
        return new List<Guid>();
    }
}

// Register in Program.cs
builder.Services.AddSingleton<IApproverStrategy, TeamLeadApproverStrategy>();

// Use in templates
{
  "approverType": "TeamLead",
  "requiredApprovals": 1
}
```

### Option 2: Add Custom Validator

```csharp
// New file: Services/Validators/CustomValidator.cs
public class CustomValidator : IWorkflowValidator
{
    public string Name => "Custom";
    
    public Task<ValidationResult> ValidateAsync(WorkflowValidationContext context)
    {
        // Your validation logic
        if (/* condition */)
            return Task.FromResult(ValidationResult.Failure("Error message"));
        
        return Task.FromResult(ValidationResult.Success());
    }
}

// Register in Program.cs
builder.Services.AddScoped<ValidationPipeline>(sp =>
{
    var pipeline = new ValidationPipeline();
    pipeline.AddValidator(new FileTypeValidator());
    pipeline.AddValidator(new UserFileTypePermissionValidator(...));
    pipeline.AddValidator(new ApproverScopeValidator(...));
    pipeline.AddValidator(new CustomValidator()); // Add your validator
    return pipeline;
});
```

## 🧪 Testing Existing Workflows

To verify nothing broke:

### Test 1: Create Workflow with "Users" Approver Type
```bash
POST /api/workflow/templates
{
  "name": "Test Template",
  "levels": [{
    "order": 1,
    "approverType": "Users",
    "userIds": ["user-guid"],
    "requiredApprovals": 1,
    "allowedFileTypes": ["application/pdf"]
  }]
}
```

Expected: ✅ Works as before

### Test 2: Approve Workflow
```bash
POST /api/workflow/{instanceId}/approve?approverId={userId}
{
  "comment": "Approved",
  "signatureBase64": "..."
}
```

Expected: ✅ Same validations apply (file type, permission, approver scope)

### Test 3: Get Pending Approvals
```bash
GET /api/workflow/pending-approvals?userId={userId}
```

Expected: ✅ Returns same results as before

## 📝 Developer Notes

### For Code Maintenance

**Finding Validation Logic**:
- **Before**: Search in `WorkflowService.cs`, methods `ValidateDocumentFileTypeAsync()`, etc.
- **After**: Look in `Services/Validators/` folder

**Finding Approver Logic**:
- **Before**: Search in `WorkflowService.cs`, method `IsUserInLevelScopeAsync()`
- **After**: Look in `Services/Strategies/` folder

**Adding New Feature**:
- **Before**: Modify `WorkflowService.cs` (high risk of breaking existing code)
- **After**: Add new Strategy or Validator class (zero risk to existing code)

### For Debugging

**Validation Failures**:
```csharp
// Old way: Set breakpoint in WorkflowService.ApproveAsync()
// New way: Set breakpoint in specific validator

// Example: Debug file type validation
// Breakpoint in: Services/Validators/FileTypeValidator.cs, line ~30

// Example: Debug permission check
// Breakpoint in: Services/Validators/UserFileTypePermissionValidator.cs, line ~35
```

**Approver Resolution**:
```csharp
// Old way: Set breakpoint in WorkflowService.IsUserInLevelScopeAsync()
// New way: Set breakpoint in specific strategy

// Example: Debug "Users" approver type
// Breakpoint in: Services/Strategies/UsersApproverStrategy.cs, line ~20
```

## 🔧 Troubleshooting

### Issue: "Strategy not found" error

**Cause**: New approver type not registered

**Solution**: 
1. Check if strategy is registered in `Program.cs`
2. Verify `ApproverType` property matches template configuration

### Issue: Validation fails with new error message

**Cause**: New validator added to pipeline

**Solution**:
1. Check `Program.cs` to see which validators are registered
2. Review validator logic in `Services/Validators/`
3. To disable a validator, remove it from pipeline in `Program.cs`

### Issue: Example strategy throws NotImplementedException

**Cause**: Example strategies in `Examples/` folder are not fully implemented

**Solution**:
1. Don't register example strategies unless you implement them first
2. Implement the logic in the strategy class
3. Or create your own strategy instead of using examples

## 📚 Further Reading

- **EXTENSIBILITY_GUIDE.md**: Complete guide to extending the system
- **IMPROVEMENTS.md**: Summary of improvements and benefits
- **README_VN.md**: Updated main documentation

## 🎯 Summary

### For Users
- ✅ Nothing changed - workflows work exactly as before
- ✅ Same API endpoints
- ✅ Same behavior

### For Developers
- ✅ Cleaner, more maintainable code
- ✅ Easy to add new features
- ✅ Better testability
- ✅ Follows best practices (SOLID principles)

### For System Administrators
- ✅ No migration needed
- ✅ Same deployment process
- ✅ Same monitoring and logging
- ✅ New extension capabilities available when needed

---

**Bottom Line**: This is an **internal refactoring** that improves code quality while maintaining 100% backward compatibility. Existing workflows continue to work unchanged, but the system is now much easier to extend for future requirements.
