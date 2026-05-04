# 15-Minute Completion Checklist

## ? Already Done (Automated)
- [x] All 18 C# files migrated to WebAppExperimental26 namespace
- [x] All using directives updated
- [x] LoggingHelper namespace fixed
- [x] DataServiceInitialCalls.cs cleaned up
- [x] Template documentation created
- [x] Setup scripts created

## ?? Your Manual Tasks (15 minutes)

### Task 1: Edit .csproj (5 minutes)

Open: `WebAppExperimental26\WebAppExperimental26.csproj`

**Step 1:** Delete line 19:
```xml
? DELETE: <ProjectReference Include="..\libs\red-rfid-aclj-project01SEP2025C\REDRFID\REDRFID\REDRFID.csproj" />
```

**Step 2:** Find line 63-64 and add closing tag:
```xml
<Target Name="CalculateHashes" AfterTargets="PrepublishConfigurationDownload;Build">
    <Exec Command="powershell..." />
</Target>  ? ADD THIS
```

Save the file.

### Task 2: Add NuGet Packages (5 minutes)

Open PowerShell in project directory:
```powershell
cd WebAppExperimental26
dotnet add package Microsoft.Azure.Cosmos
dotnet add package Microsoft.EntityFrameworkCore.Cosmos
dotnet add package Azure.Security.KeyVault.Secrets
dotnet add package Azure.Identity
```

### Task 3: Build & Verify (5 minutes)

```powershell
dotnet clean
dotnet restore
dotnet build
```

**Expected result:** ? Build succeeded with 0 errors

If build fails, check:
- `.csproj` changes saved?
- All 4 packages added?
- Internet connection for package restore?

### Task 4: Test Run (Optional - 2 minutes)

```powershell
dotnet run
```

**Expected result:** App starts on https://localhost:7xxx

Press `Ctrl+C` to stop.

## ?? Success Criteria

- [ ] .csproj has no `REDRFID.csproj` reference
- [ ] .csproj `<Target>` tag is closed
- [ ] 4 NuGet packages added
- [ ] `dotnet build` succeeds
- [ ] No compiler errors
- [ ] App runs successfully

## ?? When Complete

You'll have a **standalone template project** ready for:
- New Razor Pages projects
- Azure integration (optional via feature flags)
- Enterprise development patterns
- Teaching and demonstrations

---

**Time Estimate**: 15 minutes  
**Complexity**: Low (just edits and package adds)  
**Risk**: None (REDRFID project untouched)
