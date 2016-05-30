using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using LitJson;
using System.IO;
using ZXing;
using ZXing.QrCode;
using UnityEngine.UI;

using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;


public class WxCode : MonoBehaviour {
    public delegate void Finish<T>(T t);
    private int CodeWidth = 256;
    private int CodeHeight = 256;
	// Use this for initialization
	void Start () {
        //CreateWeChaQRCode("10001");
        CreateAlipayQRCode();
    }

    //生成支付宝二维码
    public void CreateAlipayQRCode() {
        //支付宝网关地址
        // -----沙箱地址-----
        string serverUrl = "http://openapi.alipaydev.com/gateway.do";
        // -----线上地址-----
        // string serverUrl = "https://openapi.alipay.com/gateway.do";
        //应用ID
        string appId = "2013092500031084";
        //商户私钥
        string privateKeyPem = GetCurrentPath() + "aop-sandbox-RSA-private-c#.pem";
        string publicKeyPem = GetCurrentPath() + "public-key.pem";
        IAopClient client = new DefaultAopClient(serverUrl, appId, privateKeyPem, "json", "RSA", publicKeyPem, "GBK");
        AlipayTradePrecreateRequest request = new AlipayTradePrecreateRequest();
       
        request.BizContent = @"  {
            'out_trade_no':'20150320010101001',
            'seller_id':'2088102146225135',
            'total_amount':88.88,
            'discountable_amount':8.88,
            'undiscountable_amount':80,
            'buyer_logon_id':'15901825620',
            'subject':'Iphone6 16G',
            'body':'Iphone6 16G',
              'goods_detail':[{
                        'goods_id':'apple-01',
                'alipay_goods_id':'20010001',
                'goods_name':'ipad',
                'quantity':1,
                'price':2000,
                'goods_category':'34543238',
                'body':'特价手机'
                }],
            'operator_id':'yx_001',
            'store_id':'NJ_001',
            'terminal_id':'NJ_T_001',
            'extend_params':{
              'sys_service_provider_id':'2088511833207846'
            },
            'timeout_express':'90m',
            'royalty_info':{
              'royalty_type':'ROYALTY',
                'royalty_detail_infos':[{
                            'serial_no':1,
                  'trans_in_type':'userId',
                  'batch_no':'123',
                  'out_relation_id':'20131124001',
                  'trans_out_type':'userId',
                  'trans_out':'2088101126765726',
                  'trans_in':'2088101126708402',
                  'amount':0.1,
                  'desc':'分账测试1'
                  }]
            }
          }
        ";
        AlipayTradePrecreateResponse response = client.Execute(request);
        Sprite codeSprite = CreateQRSprite(response.Code, CodeWidth, CodeHeight);
        gameObject.GetComponent<Image>().sprite = codeSprite;
        gameObject.SetActive(true);
    }
    //生成微信二维码
    public void CreateWeChaQRCode(string productId)
    {
        WxPayData data = new WxPayData();
        data.SetValue("body", "test");//商品描述
        data.SetValue("attach", "test");//附加数据
        data.SetValue("out_trade_no", GenerateOutTradeNo());//随机字符串
        data.SetValue("total_fee", 1);//总金额
        data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
        data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
        data.SetValue("goods_tag", "jjj");//商品标记
        data.SetValue("trade_type", "NATIVE");//交易类型
        data.SetValue("product_id", productId);//商品ID

        StartCoroutine(CallWeCha(data, delegate (WxPayData wxpaydata)//调用统一下单接口
        {
            string url = wxpaydata.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接
            Sprite codeSprite = CreateQRSprite(url, CodeWidth, CodeHeight);
            gameObject.GetComponent<Image>().sprite = codeSprite;
            gameObject.SetActive(true);
        }));
    }
    //根据返回的二维码code生成二维码sprite
    private Sprite CreateQRSprite(string code, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        var color32 = Encode(code, texture.width, texture.height);
        texture.SetPixels32(color32);
        texture.Apply();
        Rect rect = new Rect(0f, 0f, texture.width, texture.height);
        return Sprite.Create(texture, rect, Vector2.one);
    }
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    //微信 统一下单API
    public IEnumerator CallWeCha(WxPayData inputObj, Finish<WxPayData> finish){
        string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
        //检测必填参数
        if (!inputObj.IsSet("out_trade_no"))
        {
            Debug.Log("缺少统一支付接口必填参数out_trade_no！");
            yield break;
        }
        if (!inputObj.IsSet("body"))
        {
            Debug.Log("缺少统一支付接口必填参数body！");
            yield break;
        }
        if (!inputObj.IsSet("total_fee"))
        {
            Debug.Log("缺少统一支付接口必填参数total_fee！");
            yield break;
        }
        else if (!inputObj.IsSet("trade_type"))
        {
            Debug.Log("缺少统一支付接口必填参数trade_type！");
            yield break;
        }

        //关联参数
        if (inputObj.GetValue("trade_type").ToString() == "JSAPI" && !inputObj.IsSet("openid"))
        {
            Debug.Log("统一支付接口中，缺少必填参数openid！trade_type为JSAPI时，openid为必填参数！");
            yield break;
        }
        if (inputObj.GetValue("trade_type").ToString() == "NATIVE" && !inputObj.IsSet("product_id"))
        {
            Debug.Log("统一支付接口中，缺少必填参数product_id！trade_type为JSAPI时，product_id为必填参数！");
            yield break;
        }

        //异步通知url未设置，则使用配置文件中的url
        if (!inputObj.IsSet("notify_url"))
        {
            inputObj.SetValue("notify_url", WxPayConfig.NOTIFY_URL);//异步通知url
        }

        inputObj.SetValue("appid", WxPayConfig.APPID);//公众账号ID
        inputObj.SetValue("mch_id", WxPayConfig.MCHID);//商户号
        inputObj.SetValue("spbill_create_ip", WxPayConfig.IP);//终端ip

        inputObj.SetValue("nonce_str", GenerateNonceStr());//随机字符串

        //签名
        inputObj.SetValue("sign", inputObj.MakeSign());
        string xml = inputObj.ToXml();

        var start = DateTime.Now;

        Debug.Log(xml);
        WWWForm form = new WWWForm();
        form.AddField("aaaa", xml);
        WWW upLoad = new WWW(url, form);
        yield return upLoad;
        print(upLoad.text);
        WxPayData result = new WxPayData();
        result.FromXml(upLoad.text);
        finish(result);
    }
    /**
         * 生成随机串，随机串包含字母或数字
         * @return 随机串
         */
    public static string GenerateNonceStr()
    {
        return Guid.NewGuid().ToString().Replace("-", "");
    }
    /**
       * 根据当前系统时间加随机序列来生成订单号
        * @return 订单号
       */
    public static string GenerateOutTradeNo()
    {
        var ran = new System.Random();
        return string.Format("{0}{1}{2}", WxPayConfig.MCHID, DateTime.Now.ToString("yyyyMMddHHmmss"), ran.Next(999));
    }
    private static string GetCurrentPath()
    {
        string basePath = System.IO.Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName;
        return basePath + "/Test/";
    }
}
