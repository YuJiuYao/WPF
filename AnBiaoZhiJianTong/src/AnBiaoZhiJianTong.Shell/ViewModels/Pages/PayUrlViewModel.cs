using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnBiaoZhiJianTong.Common.Utilities;
using AnBiaoZhiJianTong.Core.Contracts.Runtime;
using AnBiaoZhiJianTong.Shell.Models;
using AnBiaoZhiJianTong.Shell.Views.CustomGeneralDialogs;
using CefSharp.Wpf;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;



namespace AnBiaoZhiJianTong.Shell.ViewModels.Pages
{
    public class PayUrlViewModel : BindableBase
    {

        public DelegateCommand PayLoadCommand { get; private set; }

        private readonly IEventAggregator _eventAggregator;
        private readonly IAppDataBus _appDataBus;
        private SubscriptionToken _LoginsubscriptionToken;
        private SubscriptionToken _Paysubscriptiontoken;

        public static string specialZoneName = ConfigurationManager.AppSettings["SzGuid"];
        private string guid;
        private string timestamp;
        private string deviceFingerprint;
        private string AppId;
        private string nonce;
        private string signature;
        private string InstanceGuid;
        private string Statue;
        private string Remark = "";
        private string _token; // 假设你在这里存储你的Token
        public string Token
        {
            get => _token;
            set => SetProperty(ref _token, value);
        }
        public List<string> Data { get; set; }
        private string _payUrlTemplate =KeyEncryption.DecryptAes(ConfigurationManager.AppSettings["ZfDXGF2rdYuvWaOKdafrlw=="]);// 自定义的URL解密,key和Iv

        private String _payUrl;
        public string PayUrl
        {
            get { return _payUrl; }
            set { SetProperty(ref _payUrl, value); }
        }

        public static Dictionary<string, string> parameters;
        //检查核销Api
        public static string OutPayTemplate = KeyEncryption.DecryptAes(ConfigurationManager.AppSettings["LHJmnMSkWgcK2C57OhuVMw=="]) ;
        private string _outPayUrl;
        public string OutPayUrl
        {
            get { return _outPayUrl; }
            set { SetProperty(ref _outPayUrl, value); }
        }
        //更新核销Api
        public static string UpdataPayTemplate = KeyEncryption.DecryptAes(ConfigurationManager.AppSettings["92H6RkypQMEVrdC2DZpgCw=="]);
        private string _upadteUrl;
        public string UpadteUrl
        {
            get { return _upadteUrl; }
            set { SetProperty(ref _upadteUrl, value); }
        }
        //核销支付结果Api
        public static string UrlCheckTemplate = KeyEncryption.DecryptAes(ConfigurationManager.AppSettings["+yWG0tuio5WYmr/ZsziUVQ=="]);
        private string _urlCheck;
        public string UrlCheck
        {
            get { return _urlCheck; }
            set { SetProperty(ref _urlCheck, value); }
        }

        private string _memberGuid;
        public string MemberGuid
        {
            get { return _memberGuid; }
            set { SetProperty(ref _memberGuid, value); }
        }
        string uiid = "", Captchacode = "";
        private string _szapGuid;
        public string SzapGuid
        {
            get { return _szapGuid; }
            set { SetProperty(ref _szapGuid, value); }
        }
        public static string relativePath = @"private.pem";
        public static string fullPath = Path.Combine(AppContext.BaseDirectory, "configs", relativePath);
        public static readonly RSA PrivateKey = LoadPrivateKeyFromFile(fullPath);
        public List<string> PayOutMessage;
        private System.Timers.Timer _timer;
        private readonly ChromiumWebBrowser _chromiumBrowser;
        private List<string> _lastPayInfo;
        public PayUrlViewModel(IEventAggregator eventAggregator, ChromiumWebBrowser chromiumBrowser, IAppDataBus appDataBus)
        {
            _chromiumBrowser = chromiumBrowser;
            _eventAggregator = eventAggregator;
            _appDataBus = appDataBus;
            //_Paysubscriptiontoken = _eventAggregator.GetEvent<PayUrlInfoEvent>().Subscribe(PayLoginInfo, ThreadOption.UIThread ,false);
            //PayLoginInfo(CurrentPayMessage.ToList());
            _chromiumBrowser.Address = PayUrl;
        }
        private void PayLoginInfo(List<string> PayInfo)
        {
            if (PayInfo == null || PayInfo.Count < 7)
            {
                MessageBox.Show("PayInfo is invalid or incomplete.");
                return;
            }
            Data = PayInfo;
            guid = Data[1];
            nonce = GenerateNonce();
            timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();;
            deviceFingerprint = Data[2];
            AppId = Data[4];
            _token = _appDataBus.Get<string>("Token");
            signature = GenerateSignature(nonce, timestamp);
            _appDataBus.Set("Appid",AppId);
            _appDataBus.Set("deviceFingerprint", deviceFingerprint);
            _lastPayInfo = Data;
            LoadPayPage();
        }
        private static RSA LoadPrivateKeyFromFile(string path)
        {
            return RsaKeyHelper.LoadPrivateKey(path);
        }
        private void LoadPayPage()
        {
            try
            {
                PayUrl = GeneratePayUrl(guid, specialZoneName, timestamp, deviceFingerprint, AppId, nonce, _token);
            }
            catch (Exception ex)
            {
                var resp = CommonDialogWindow.Show(new CommonDialogOptions
                {
                    Title = "加载失败",
                    Header = $"网页加载时出错: {ex.Message}",
                    Message = "网络异常，请重试。",
                    Icon = CommonDialogIcon.Error,
                    Owner = Application.Current?.MainWindow,
                });
                MessageBox.Show($"网页加载时出错: {ex.Message}");
                return;
            }
        }
        public string GeneratePayUrl(string guid, string specialZoneName, string timestamp, string deviceFingerprint, string appId, string nonce, string token)
        {
            string payUrl = _payUrlTemplate
                .Replace("{Guid}", guid)
                .Replace("{SpecialZoneName}", Uri.EscapeDataString(specialZoneName))
                .Replace("{Timestamp}", timestamp)
                .Replace("{DeviceFingerprint}", deviceFingerprint)
                .Replace("{AppId}", appId)
                .Replace("{Nonce}", nonce)
                .Replace("{Token}", token);

            return payUrl;
        }
        public string GenerateSignature(string nonce, string timestamp)
        {
            parameters = new Dictionary<string, string>
            {
                { "Zhijiantong-APPID", AppId },
                { "Zhijiantong-Nonce", nonce },
                { "Zhijiantong-Timestamp", timestamp },
                {"Zhijiantong-Devicefingerprint",deviceFingerprint}
            };
            // 按key排序并拼接成字符串
            string sortedParams = string.Join("&", parameters.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value}"));

            // 使用RSA2048进行签名
            string signature = Rsa2048Sign(sortedParams);
            return signature;
        }
        public static string Rsa2048Sign(string data)
        {
            // 使用之前生成的私钥参数进行签名
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = PrivateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signatureBytes);
        }
        private static string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }
        //发起核销流程
        public async Task<bool> UserPayCheck()
        {
            try
            {
                nonce = GenerateNonce();
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
                signature = GenerateSignature(nonce, timestamp);
                var requestData = new
                {
                    memberGuid = guid,
                    specialZoneId = ConfigurationManager.AppSettings["SzGuid"]
                };
                string json = JsonConvert.SerializeObject(requestData);
                using (HttpClient client = new HttpClient())
                {
                    UrlCheck = UrlCheckTemplate;

                    client.DefaultRequestHeaders.Add("Zhijiantong-Nonce", nonce);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Signature", signature);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Timestamp", timestamp);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Signature-Type", "sha256");
                    client.DefaultRequestHeaders.Add("Zhijiantong-APPID", AppId);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Devicefingerprint", deviceFingerprint);
                    client.DefaultRequestHeaders.Add("Authorization", _token);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(UrlCheck, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<PayCheck>(responseJson);
                        if (result.StatusCode == 200)
                        {
                            if (result.Data != null)
                            {
                                PayOutMessage = new List<string>
                                {
                                    result.Data.Status,
                                    result.Data.chekoutId,
                                    result.Message,
                                };
                                InstanceGuid = result.Data.chekoutId;
                                _appDataBus.Set("InstanceGuid", InstanceGuid);
                                Statue = result.Data.Status;
                                _appDataBus.Set("Statue", Statue);
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"支付校验出错: {ex.Message}");
                return false;
            }
        }
        //查询核销状态
        public async Task<bool> PayCheckResult()
        {
            try
            {
                string nonce = GenerateNonce();
                string timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
                string signature = GenerateSignature(nonce, timestamp);
                _appDataBus.TryGet("_token", out _token);
                using (HttpClient client = new HttpClient())
                {
                    OutPayUrl = GenerateOutUrl(InstanceGuid);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Nonce", nonce);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Signature", signature);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Timestamp", timestamp);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Signature-Type", "sha256");
                    client.DefaultRequestHeaders.Add("Zhijiantong-APPID", AppId);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Devicefingerprint", deviceFingerprint);
                    client.DefaultRequestHeaders.Add("Authorization", _token);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync(OutPayUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<PayCheck>(responseJson);
                        if (result.StatusCode == 200)
                        {
                            if (result.Data != null)
                            {
                                InstanceGuid = result.Data.chekoutId;
                                Statue = result.Data.Status;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        return false;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //更新核销状态
        public async Task<bool> UpadataCheckResult()
        {
            try
            {
                if (Statue != null)
                {
                    Statue = "2";
                    string Remark = "核销完成";
                }
                else
                {
                    string Remark = "未核销";
                }
                string nonce = GenerateNonce();
                string timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
                string signature = GenerateSignature(nonce, timestamp);
                _appDataBus.TryGet("_token", out _token);
                var putData = new
                {
                    instanceGuid = InstanceGuid,
                    status = Statue,
                    remark = Remark
                };
                string json = JsonConvert.SerializeObject(putData);
                using (HttpClient client = new HttpClient())
                {
                    UpadteUrl = UpdataPayTemplate;
                    client.DefaultRequestHeaders.Add("Zhijiantong-Nonce", nonce);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Signature", signature);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Timestamp", timestamp);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Signature-Type", "sha256");
                    client.DefaultRequestHeaders.Add("Zhijiantong-APPID", AppId);
                    client.DefaultRequestHeaders.Add("Zhijiantong-Devicefingerprint", deviceFingerprint);
                    client.DefaultRequestHeaders.Add("Authorization", _token);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(UpadteUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<PayCheck>(responseJson);
                        if (result.StatusCode == 200)
                        {
                            if (result.Data != null)
                            {
                                InstanceGuid = result.Data.chekoutId;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        return false;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public string GenerateOutUrl(string instanceGuid)
        {
            string OutUrl = OutPayTemplate
                .Replace("{instanceGuid}", instanceGuid);
            return OutUrl;
        }
    }

    public class PayCheck
    {
        [JsonProperty("code")]
        public int StatusCode { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public CheckoutData Data { get; set; }
    }
    public class CheckoutData
    {
        [JsonProperty("instanceGuid")]
        public string chekoutId { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
