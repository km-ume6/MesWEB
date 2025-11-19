<#
PowerShell スクリプト: 環境変数の登録・更新・削除（対話型対応）

機能:
 - 対話モード: 実行するとメニューが表示され、設定・表示・削除が行えます。
 - 非対話モード: スクリプトパラメーターを使って自動実行可能。

非対話使用例:
  .\set-sqlserver-env.ps1 -ConnectionString 'Server=sql;Database=DB;User Id=sa;Password=pass;Encrypt=False;' -Scope User
  .\set-sqlserver-env.ps1 -Remove -Scope Machine

対話使用例:
  .\set-sqlserver-env.ps1
  -> メニューに従って選択してください。

このスクリプトは互換性のために2つのキーを扱います:
  SQLSERVER_CONNECTIONSTRING
  ConnectionStrings__Default

注意:
 - Process スコープは現在のセッションのみ有効です。
 - User/Machine スコープは永続化されます（Machine は管理者権限が必要）。

エンコーディングについて:
 - 実行時に文字化けする場合、PowerShell のコンソールの出力エンコーディングが UTF-8 でないことが原因です。
 - スクリプトは実行時にコンソールの入出力を UTF-8 に切り替えますが、Windows PowerShell 5.1 ではスクリプトファイルを "UTF-8 with BOM" で保存する必要がある場合があります。
 - 可能であれば PowerShell 7 (pwsh) を使うと UTF-8 がデフォルトで扱われ、文字化けが起こりにくくなります。
#>

# 可能な限りコンソールの入出力を UTF-8 に設定して文字化けを防ぐ
try {
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    [Console]::InputEncoding = [System.Text.Encoding]::UTF8
    # Windows PowerShell 用に $OutputEncoding も設定
    $OutputEncoding = [System.Text.Encoding]::UTF8
} catch {
    # 古い環境では例外が出ることがあるが、処理は続行する
}


# Validate ScopeArg at runtime to avoid MetadataError when a same-named variable exists in the session
if ($null -eq $ScopeArg) { $ScopeArg = 'User' }
if ($ScopeArg -notin @('Process','User','Machine')) {
    Write-Host "警告: ScopeArg の値 '$ScopeArg' は無効です。User にフォールバックします。"
    $ScopeArg = 'User'
}

function Set-EnvVarPersisted([string]$name, [string]$value, [EnvironmentVariableTarget]$target) {
    try {
        [Environment]::SetEnvironmentVariable($name, $value, $target)
        return $true
    } catch {
        # 権限エラーなどを捕捉し、呼び出し側で対処できるように false を返す
        Write-Host "環境変数の永続化に失敗しました: $name (Target=$target) - $($_.Exception.Message)"
        return $false
    }
}

function Remove-EnvVarPersisted([string]$name, [EnvironmentVariableTarget]$target) {
    try {
        [Environment]::SetEnvironmentVariable($name, $null, $target)
        return $true
    } catch {
        Write-Host "環境変数の削除に失敗しました: $name (Target=$target) - $($_.Exception.Message)"
        return $false
    }
}

function Show-CurrentValues {
    Write-Host "Current (Process):"
    Write-Host "  SQLSERVER_CONNECTIONSTRING = $($env:SQLSERVER_CONNECTIONSTRING)"
    Write-Host "  ConnectionStrings__Default = $($env:ConnectionStrings__Default)"
    Write-Host "\nPersisted (User / Machine):"
    $userVal1 = [Environment]::GetEnvironmentVariable('SQLSERVER_CONNECTIONSTRING',[EnvironmentVariableTarget]::User)
    $userVal2 = [Environment]::GetEnvironmentVariable('ConnectionStrings__Default',[EnvironmentVariableTarget]::User)
    Write-Host "  [User] SQLSERVER_CONNECTIONSTRING = $userVal1"
    Write-Host "  [User] ConnectionStrings__Default = $userVal2"
    $machineVal1 = [Environment]::GetEnvironmentVariable('SQLSERVER_CONNECTIONSTRING',[EnvironmentVariableTarget]::Machine)
    $machineVal2 = [Environment]::GetEnvironmentVariable('ConnectionStrings__Default',[EnvironmentVariableTarget]::Machine)
    Write-Host "  [Machine] SQLSERVER_CONNECTIONSTRING = $machineVal1"
    Write-Host "  [Machine] ConnectionStrings__Default = $machineVal2"
}

function Prompt-ForScope {
    while ($true) {
        $choice = Read-Host "Scope? (P)rocess / (U)ser / (M)achine [default U]"
        if ([string]::IsNullOrWhiteSpace($choice)) { return 'User' }
        switch ($choice.ToUpper()) {
            'P' { return 'Process' }
            'U' { return 'User' }
            'M' { return 'Machine' }
            default { Write-Host "無効な選択です。P, U, M のいずれかを入力してください。" }
        }
    }
}

function Prompt-ForConnectionStringFields {
    Write-Host "個別入力モード: 各項目を入力してください。Enterで既定値または空。"

    $server = Read-Host "Server (例: myserver または myserver.domain.com) [必須]"
    if ([string]::IsNullOrWhiteSpace($server)) {
        Write-Host "Server は必須です。キャンセルします。"
        return $null
    }

    $port = Read-Host "Port [1433]"
    if ([string]::IsNullOrWhiteSpace($port)) { $port = '1433' }
    $serverPart = if ($port -ne '1433' -and -not [string]::IsNullOrWhiteSpace($port)) { "$server,$port" } else { $server }

    $database = Read-Host "Database [CrystalGrowthNotebook2]"
    if ([string]::IsNullOrWhiteSpace($database)) { $database = 'CrystalGrowthNotebook2' }

    $authChoice = Read-Host "認証方式: (W)indows / (S)QL Login [S]"
    if ([string]::IsNullOrWhiteSpace($authChoice)) { $authChoice = 'S' }
    $authChoice = $authChoice.Substring(0,1).ToUpper()

    $parts = @()
    $parts += "Server=$serverPart"
    $parts += "Database=$database"

    if ($authChoice -eq 'W') {
        $parts += "Trusted_Connection=True"
    } else {
        $user = Read-Host "User Id"
        if ([string]::IsNullOrWhiteSpace($user)) {
            Write-Host "User Id が空です。SQL認証を選ぶ場合は User Id を指定してください。キャンセルします。"
            return $null
        }
        $securePwd = Read-Host "Password (入力は非表示)" -AsSecureString
        $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePwd)
        $pwdPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)

        $parts += "User Id=$user"
        $parts += "Password=$pwdPlain"
    }

    $encrypt = Read-Host "Encrypt? (y/N) [N]"
    $encrypt = if ($encrypt -and $encrypt.Substring(0,1).ToLower() -eq 'y') { 'True' } else { 'False' }
    $parts += "Encrypt=$encrypt"

    $trust = Read-Host "TrustServerCertificate? (Y/n) [Y]"
    $trust = if ($trust -and $trust.Substring(0,1).ToLower() -eq 'n') { 'False' } else { 'True' }
    $parts += "TrustServerCertificate=$trust"

    $mars = Read-Host "MultipleActiveResultSets? (Y/n) [Y]"
    $mars = if ($mars -and $mars.Substring(0,1).ToLower() -eq 'n') { 'False' } else { 'True' }
    $parts += "MultipleActiveResultSets=$mars"

    $conn = ($parts -join ';')
    return $conn
}

function Test-ConnectionString([string]$connectionString) {
    # テスト用に短めのタイムアウトを付与（既に指定がなければ）
    $hasTimeout = $connectionString -match '(?i)\b(connect\s*timeout|connection\s*timeout)\s*=\s*\d+'
    $testConn = $connectionString
    if (-not $hasTimeout) { $testConn = "$connectionString;Connect Timeout=5" }

    try {
        # System.Data.SqlClient を使って接続テスト
        $conn = New-Object System.Data.SqlClient.SqlConnection($testConn)
        $conn.Open()
        $conn.Close()
        Write-Host "接続テスト: 成功"
        return $true
    } catch {
        Write-Host "接続テスト: 失敗 - $($_.Exception.Message)"
        return $false
    }
}

# If non-interactive parameters provided, perform them and exit
if ($NonInteractive -or $PSBoundParameters.Count -gt 0) {
    if ($Remove) {
        if ($ScopeArg -eq 'Process') {
            Remove-Item Env:SQLSERVER_CONNECTIONSTRING -ErrorAction SilentlyContinue
            Remove-Item Env:ConnectionStrings__Default -ErrorAction SilentlyContinue
            Write-Host "Removed process environment variables."
        } else {
            $target = [EnvironmentVariableTarget]::User
            if ($ScopeArg -eq 'Machine') { $target = [EnvironmentVariableTarget]::Machine }
            $ok1 = Remove-EnvVarPersisted -name 'SQLSERVER_CONNECTIONSTRING' -target $target
            $ok2 = Remove-EnvVarPersisted -name 'ConnectionStrings__Default' -target $target
            if ($ok1 -and $ok2) {
                Write-Host "Removed persisted environment variables ($ScopeArg)."
            } else {
                Write-Host "一部または全ての環境変数の削除に失敗しました。管理者権限が必要な場合は PowerShell を管理者として再実行してください。"
            }
        }
        return
    }

    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        Write-Error "ConnectionString を指定してください。-ConnectionString '...'"
        return
    }

    if ($ScopeArg -eq 'Process') {
        $env:SQLSERVER_CONNECTIONSTRING = $ConnectionString
        $env:ConnectionStrings__Default = $ConnectionString
        Write-Host "Set process environment variables."
    } else {
        $target = [EnvironmentVariableTarget]::User
        if ($ScopeArg -eq 'Machine') { $target = [EnvironmentVariableTarget]::Machine }
        $ok1 = Set-EnvVarPersisted -name 'SQLSERVER_CONNECTIONSTRING' -value $ConnectionString -target $target
        $ok2 = Set-EnvVarPersisted -name 'ConnectionStrings__Default' -value $ConnectionString -target $target
        if ($ok1 -and $ok2) {
            Write-Host "Persisted environment variables set ($ScopeArg)."
            if ($ScopeArg -eq 'User') {
                Write-Host "注: 既存のログインセッションでは反映されない場合があります。新しいセッションで確認してください。"
            }
        } else {
            # 失敗時は Machine スコープでの権限不足を案内
            if ($target -eq [EnvironmentVariableTarget]::Machine) {
                Write-Host "環境変数の永続化に失敗しました: Machine スコープは管理者権限が必要です。PowerShell を管理者として再実行するか、User/Process スコープを使用してください。"
            } else {
                Write-Host "環境変数の永続化に失敗しました。Process スコープの使用を検討してください。"
            }
        }
    }
    return
}

# Interactive menu
while ($true) {
    Write-Host "\n=== SQL Server 環境変数設定ツール ==="
    Write-Host "1) 設定 (Set)"
    Write-Host "2) 表示 (Show current)"
    Write-Host "3) 削除 (Remove)"
    Write-Host "4) 終了 (Exit)"
    $sel = Read-Host "選択してください (1-4)"

    switch ($sel) {
        '1' {
            # 個別入力モードの選択
            $modeChoice = Read-Host "接続文字列をフィールド毎に入力しますか? (Y/n) [Y]"
            if ([string]::IsNullOrWhiteSpace($modeChoice) -or $modeChoice.Substring(0,1).ToLower() -ne 'n') {
                $conn = Prompt-ForConnectionStringFields
                if ($null -eq $conn) { Write-Host "設定をキャンセルしました。"; continue }
            } else {
                $conn = Read-Host "接続文字列を入力してください (例: Server=host,1433;Database=DB;User Id=sa;Password=pass;Encrypt=False;)"
                if ([string]::IsNullOrWhiteSpace($conn)) { Write-Host "接続文字列が空です。キャンセルします。"; continue }
            }

            # 接続テストを自動実行
            Write-Host "接続テストを実行します..."
            $testOk = Test-ConnectionString $conn
            if (-not $testOk) {
                $saveAnyway = Read-Host "接続に失敗しました。保存しますか? (y/N) [N]"
                if ([string]::IsNullOrWhiteSpace($saveAnyway) -or $saveAnyway.Substring(0,1).ToLower() -ne 'y') {
                    Write-Host "保存をキャンセルしました。"; continue
                }
            }

            $scopeChoice = Prompt-ForScope
            Write-Host "以下の内容で設定します:"
            Write-Host "  Scope: $scopeChoice"
            Write-Host "  ConnectionString: $conn"
            $ok = Read-Host "実行しますか？ (Y/n)"
            if ($ok -and $ok.ToLower().StartsWith('n')) { Write-Host "キャンセルしました。"; continue }

            if ($scopeChoice -eq 'Process') {
                $env:SQLSERVER_CONNECTIONSTRING = $conn
                $env:ConnectionStrings__Default = $conn
                Write-Host "Process スコープに設定しました。"
            } else {
                $target = [EnvironmentVariableTarget]::User
                if ($scopeChoice -eq 'Machine') { $target = [EnvironmentVariableTarget]::Machine }
                $ok1 = Set-EnvVarPersisted -name 'SQLSERVER_CONNECTIONSTRING' -value $conn -target $target
                $ok2 = Set-EnvVarPersisted -name 'ConnectionStrings__Default' -value $conn -target $target
                if ($ok1 -and $ok2) {
                    Write-Host "Persisted ($scopeChoice) に設定しました。"
                    if ($scopeChoice -eq 'User') { Write-Host "注: 既存のセッションでは反映されないことがあります。新しいセッションで確認してください。" }
                } else {
                    if ($target -eq [EnvironmentVariableTarget]::Machine) {
                        Write-Host "環境変数の永続化に失敗しました: Machine スコープは管理者権限が必要です。PowerShell を管理者として再実行するか、User/Process スコープを使用してください。"
                    } else {
                        Write-Host "環境変数の永続化に失敗しました。Process スコープの使用を検討してください。"
                    }
                }
            }
        }
        '2' {
            Show-CurrentValues
        }
        '3' {
            $scopeChoice = Prompt-ForScope
            $confirm = Read-Host "本当に削除しますか？ (Y/n)"
            if ($confirm -and $confirm.ToLower().StartsWith('n')) { Write-Host "削除をキャンセルしました。"; continue }
            if ($scopeChoice -eq 'Process') {
                Remove-Item Env:SQLSERVER_CONNECTIONSTRING -ErrorAction SilentlyContinue
                Remove-Item Env:ConnectionStrings__Default -ErrorAction SilentlyContinue
                Write-Host "Process スコープから削除しました。"
            } else {
                $target = [EnvironmentVariableTarget]::User
                if ($scopeChoice -eq 'Machine') { $target = [EnvironmentVariableTarget]::Machine }
                $ok1 = Remove-EnvVarPersisted -name 'SQLSERVER_CONNECTIONSTRING' -target $target
                $ok2 = Remove-EnvVarPersisted -name 'ConnectionStrings__Default' -target $target
                if ($ok1 -and $ok2) {
                    Write-Host "Persisted ($scopeChoice) から削除しました。"
                } else {
                    if ($target -eq [EnvironmentVariableTarget]::Machine) {
                        Write-Host "削除に失敗しました: Machine スコープは管理者権限が必要です。PowerShell を管理者として再実行してください。"
                    } else {
                        Write-Host "削除に失敗しました。Process スコープの使用を検討してください。"
                    }
                }
            }
        }
        '4' {
            # break
            exit
        }
        default {
            Write-Host "無効な選択です。1-4 で選んでください。"
        }
    }
}

Write-Host "終了します。"
