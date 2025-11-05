# Check Python Syntax Errors
Write-Host "?? Checking Python files for syntax errors..." -ForegroundColor Cyan

$pythonFiles = @(
    "src\Services\Chatbot\utils\jwt_helper.py",
    "src\Services\Chatbot\routers\chat.py",
    "src\Services\Chatbot\lg\state.py",
    "src\Services\Chatbot\lg\tools\cart.py",
    "src\Services\Chatbot\lg\tools\product.py",
    "src\Services\Chatbot\lg\nodes\tools.py",
    "src\Services\Chatbot\lg\nodes\search.py"
)

$errors = 0

foreach ($file in $pythonFiles) {
    Write-Host "`nChecking: $file" -ForegroundColor Yellow
    
    if (Test-Path $file) {
$result = python -m py_compile $file 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ? OK" -ForegroundColor Green
        } else {
         Write-Host "  ? SYNTAX ERROR:" -ForegroundColor Red
Write-Host $result
  $errors++
 }
    } else {
        Write-Host "  ??  FILE NOT FOUND" -ForegroundColor Red
        $errors++
    }
}

Write-Host "`n" + ("=" * 80)
if ($errors -eq 0) {
    Write-Host "? All Python files are valid!" -ForegroundColor Green
} else {
    Write-Host "? Found $errors file(s) with errors. Please fix them." -ForegroundColor Red
    Write-Host "`n?? To fix indentation errors:" -ForegroundColor Yellow
    Write-Host "  1. Open the file in VS Code or PyCharm"
    Write-Host "  2. Select all (Ctrl+A)"
    Write-Host "  3. Format document (Shift+Alt+F in VS Code)"
    Write-Host "  4. Or manually copy code from chat history"
}
