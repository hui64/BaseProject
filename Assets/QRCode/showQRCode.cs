using System.Threading;
using UnityEngine;
using ZXing;

public class WebCameraScript : MonoBehaviour
{
    public string LastResult;
    public string Lastresult;
    public Color32[] data;
    private bool isQuit;

    public GUITexture myCameraTexture;
    private WebCamTexture webCameraTexture;

    private void Start()
    {
        //  bool success = CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        // Checks how many and which cameras are available on the device
        for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
        {
            // We want the back camera
            if (!WebCamTexture.devices[cameraIndex].isFrontFacing)
            {
                //webCameraTexture = new WebCamTexture(cameraIndex, Screen.width, Screen.height);
                webCameraTexture = new WebCamTexture(cameraIndex, 200, 200);

                // Here we flip the GuiTexture by applying a localScale transformation
                // works only in Landscape mode
                myCameraTexture.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        // Here we tell that the texture of coming from the camera should be applied 
        // to our GUITexture. As we have flipped it before the camera preview will have the 
        // correct orientation
        myCameraTexture.texture = webCameraTexture;
        // Starts the camera
        webCameraTexture.Play();
        //enabled=WebCamTexture.s
    }

    public void ShowCamera()
    {
        myCameraTexture.GetComponent<GUITexture>().enabled = true;
        webCameraTexture.Play();
    }

    public void HideCamera()
    {
        myCameraTexture.GetComponent<GUITexture>().enabled = false;
        webCameraTexture.Stop();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(60, 30 * 1, Screen.width, 20), "LastResult:" + LastResult);
        if (GUI.Button(new Rect(0, 0, 100, 100), "ON/OFF"))
        {
            if (webCameraTexture.isPlaying)
                HideCamera();
            else
                ShowCamera();
        }
    }

    private void Update()
    {
        //data = new Color32[webCameraTexture.width * webCameraTexture.height];
        data = webCameraTexture.GetPixels32();

        DecodeQR(webCameraTexture.width, webCameraTexture.height);
    }


    private void DecodeQR(int W, int H)
    {
        if (isQuit)
            return;
        // create a reader with a custom luminance source
        var barcodeReader = new BarcodeReader { AutoRotate = true, TryHarder = true };

        //        while (true)
        {
            try
            {
                // decode the current frame
                Result result = barcodeReader.Decode(data, W, H);
                if (result != null)
                {
                    LastResult = result.Text;
                    // shouldEncodeNow = true;
                    print("i read out::" + result.Text);
                }

                // Sleep a little bit and set the signal to get the next frame
                Thread.Sleep(200);
                data = null;
            }
            catch
            {
            }
        }
    }
}