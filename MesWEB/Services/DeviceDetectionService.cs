using Microsoft.JSInterop;

namespace MesWEB.Services
{
    public class DeviceDetectionService
    {
        private bool _isDetected = false;
        private bool _isAndroid = false;
        private string _userAgent = "";

        public bool IsAndroid => _isAndroid;
        public bool IsDetected => _isDetected;
        public string UserAgent => _userAgent;
        public string DeviceType => _isAndroid ? "Android" : "PC/その他";

        public event Action? DeviceDetected;

        public async Task DetectDeviceAsync(IJSRuntime jsRuntime)
        {
            // 毎回新しく判定を行う（キャッシュを無効化）
            try
            {
                // UserAgent取得
                _userAgent = await jsRuntime.InvokeAsync<string>("eval", "navigator.userAgent");
                
                // iOS判定（除外用）
                bool isiOS = _userAgent.Contains("iPhone") || 
                            _userAgent.Contains("iPad") || 
                            _userAgent.Contains("iPod");
                
                // Android判定（iOSでない場合のみ）
                bool containsAndroid = _userAgent.Contains("Android");
                
                // 安全な判定：iOS端末は絶対にAndroidとしない
                _isAndroid = containsAndroid && !isiOS;
                
                _isDetected = true;
                DeviceDetected?.Invoke(); // イベントを発火
            }
            catch (Exception)
            {
                _isAndroid = false;
                _isDetected = true;
                DeviceDetected?.Invoke();
            }
        }

        // 強制的に再検出を行うメソッド
        public async Task ForceRedetectAsync(IJSRuntime jsRuntime)
        {
            _isDetected = false;
            _isAndroid = false;
            _userAgent = "";
            await DetectDeviceAsync(jsRuntime);
        }

        // デバッグ用メソッド：手動でAndroidとして設定
        public void SetAsAndroid()
        {
            _isAndroid = true;
            _isDetected = true;
            _userAgent = "Debug: Manually set as Android";
            DeviceDetected?.Invoke();
        }

        // デバッグ用メソッド：検出状態をリセット
        public void Reset()
        {
            _isDetected = false;
            _isAndroid = false;
            _userAgent = "";
        }
    }
}