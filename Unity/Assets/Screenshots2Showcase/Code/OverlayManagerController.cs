using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// An overlay manager allows freezing the game, selecting an overlay, and making a screenshot
/// </summary>
[ExecuteInEditMode]
public class OverlayManagerController : MonoBehaviour
{
    /// <summary>
    /// The overlay child that is currently visible
    /// </summary>
    public int visibleChildIndex = 0;

    private int _lastVisibleChildIndex = 0;

    void Update()
    {
        UpdateOverlays();

        if (_isTakingScreenshots)
        {
            if (_screenshotSteps.Any())
            {
                var next = _screenshotSteps.First();
                _screenshotSteps.RemoveAt(0);
                next();
            }
            else
            {
                _isTakingScreenshots = false;
            }
        }
    }

    private static bool IsEditMode
    {
        get
        {
#if UNITY_EDITOR
            return Application.isEditor && !UnityEditor.EditorApplication.isPlaying;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// Update the scene to show the current overlay
    /// </summary>
    public void UpdateOverlays()
    {
        if (IsEditMode)
        {
            var i = 0;
            foreach (Transform c in transform)
            {
                if (c.gameObject.activeSelf && i != _lastVisibleChildIndex)
                {
                    visibleChildIndex = i;
                    break;
                }
                i++;
            }
        }

        foreach (Transform c in transform)
        {
            c.gameObject.SetActive(false);
        }

        if (_isPaused || IsEditMode)
        {
            ValidateChildIndex();
            if (transform.childCount > 0)
            {
                transform.GetChild(visibleChildIndex).gameObject.SetActive(true);
            }
        }

        _lastVisibleChildIndex = visibleChildIndex;
    }

    public void ValidateChildIndex()
    {
        var childCount = transform.childCount;

        if (childCount == 0)
        {
            visibleChildIndex = 0;
            return;
        }

        if (visibleChildIndex < 0)
        {
            visibleChildIndex = childCount - 1;
        }
        else if (visibleChildIndex > childCount - 1)
        {
            visibleChildIndex = 0;
        }
    }


    private bool _isPaused = false;
    private bool _isTakingScreenshots = false;
    private DateTime _pauseTime;
    private int _pauseScreenshotCount;

    void OnGUI()
    {
        var xCenter = Screen.width * 0.5f;

        if (!IsEditMode && !_isTakingScreenshots)
        {
            GUI.depth = -10000;

            if (!_isPaused)
            {
                if (GUI.Button(new Rect(xCenter - 50, 10, 100, 30), "FREEZE"))
                {
                    _isPaused = true;
                    UpdateOverlays();
                    Time.timeScale = 0;
                    _pauseTime = DateTime.Now;
                    _pauseScreenshotCount = 0;
                }
            }
            else
            {

                if (GUI.Button(new Rect(xCenter - 50, 10, 100, 30), "PLAY"))
                {
                    _isPaused = false;
                    UpdateOverlays();
                    Time.timeScale = 1;
                }

                if (GUI.Button(new Rect(xCenter - 50 -110, 10, 100, 30), "PREV"))
                {
                    visibleChildIndex--;
                    ValidateChildIndex();
                    UpdateOverlays();
                }

                if (GUI.Button(new Rect(xCenter - 50 + 110, 10, 100, 30), "NEXT"))
                {
                    visibleChildIndex++;
                    ValidateChildIndex();
                    UpdateOverlays();
                }

                if (GUI.Button(new Rect(xCenter - 60, 10 + 30 + 10, 120, 30), "SCREENSHOTS"))
                {
                    _isTakingScreenshots = true;
                    CreateScreenshotSteps();
                }
            }
        }


    }

    private List<Action> _screenshotSteps;
    private static bool _hasShownExplorer = false;

    void CreateScreenshotSteps()
    {
        Debug.Log("START Taking ScreenShots");

        _screenshotSteps = new List<Action>();

        var dateTimeString = _pauseTime.ToString("yyyy-MM-dd hh-mm-ss");
        string folderPath = Application.persistentDataPath + "/Screenshots/" + dateTimeString + "/";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        if (!_hasShownExplorer)
        {
            _hasShownExplorer = true;
            ExplorerHelper.ShowInExplorer(folderPath);
        }


        var originalSize = new Rect(0, 0, Screen.width, Screen.height);

        //var sizes = new List<Rect>();

        //sizes.Add(new Rect(0, 0, 800, 600));
        //sizes.Add(new Rect(0, 0, 150, 100));
        //sizes.Add(new Rect(0, 0, 100, 150));

        var sizes = ScreenshotSizeHelper.ScreenShotSizes.ToList();
        sizes.Insert(0, new Rect(0, 0, Screen.width, Screen.height));

        foreach (var s in sizes)
        {
            var size = s;
            var index = _pauseScreenshotCount;

            // Skip impossible sizes
            var resolution = Screen.currentResolution;
            if (size.width > resolution.width
                || size.height > resolution.height)
            {
                Debug.Log("Resolution not big enough for size: " + size.width + "x" + size.height);
                continue;
            }

            _screenshotSteps.Add(() =>
            {
                // Take Screenshot
                Screen.SetResolution((int)size.width, (int)size.height, false);
            });

            _screenshotSteps.Add(() =>
            {
                if (Screen.width == (int)size.width
                    && Screen.height == (int)size.height)
                {
                    Application.CaptureScreenshot(
                        folderPath
                        + index
                        + " - " + (int)size.width + "x" + (int)size.height + ".png", 1);
                }
                else
                {
                    if (Application.isEditor)
                    {
                        Debug.Log("Cannot change resolution in editor preview. Run as standalone to create screenshots.");
                    }
                    else
                    {
                        Debug.Log("Failed to change resolution to " + " - " + (int)size.width + "x" + (int)size.height);
                    }
                }
            });

            _pauseScreenshotCount++;
        }

        _screenshotSteps.Add(() =>
        {
            Screen.SetResolution((int)originalSize.width, (int)originalSize.height, false);
        });

        _screenshotSteps.Add(() =>
        {
            _isTakingScreenshots = false;

            Debug.Log("END Taking ScreenShots");
        });

    }

}

/// <summary>
/// Open Windows explorer at a certain path
/// </summary>
public static class ExplorerHelper
{
    /// <summary>
    /// Show a path in windows explorer
    /// </summary>
    /// <param name="path">The path to show</param>
    public static void ShowInExplorer(string path)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        path = path.Replace(@"/", @"\");
        //System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
        System.Diagnostics.Process.Start("explorer.exe", path);
#endif
    }
}

/// <summary>
/// Screenshot sizes
/// </summary>
public static class ScreenshotSizeHelper
{
    private static List<Rect> _screenShotSizes;

    /// <summary>
    /// A list of screenshot sizes
    /// </summary>
    public static List<Rect> ScreenShotSizes
    {
        get
        {
            if (_screenShotSizes == null)
            {
                var lines = ScreenShotsSizesText.Split('\r', '\n')
                    .Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrEmpty(l.Trim()) && !l.StartsWith("//"))
                    .ToList();

                var sizes = new List<Rect>();

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    int a;
                    int b;

                    if (parts.Length >= 2 && int.TryParse(parts[0], out a) && int.TryParse(parts[1], out b))
                    {
                        sizes.Add(new Rect(0, 0, a, b));
                        sizes.Add(new Rect(0, 0, b, a));
                    }
                    else
                    {
                        Debug.Log("Invalid screen size entry: " + line);
                    }
                }

                _screenShotSizes = sizes;
            }

            return _screenShotSizes;
        }
    }

    private static string ScreenShotsSizesText = @"
// width	height		zoom	name
// All sizes are portrait

// Unity Store
128 128

100 175
127 192
// 200 x 258

389 486
389 495
// 389 860

// Unity Ads
720 1280


// Apple
640 	960			2		iPhone4			
640 	1136		2		iPhone5			
768 	1024		1		iPad			
1536 	2048		2		iPadHi			

// Android

// From http://stackoverflow.com/questions/6272384/most-popular-screen-sizes-resolutions-on-android-phones
// http://developer.android.com/guide/practices/screens_support.html#testing

1600		2560	2		Android_XLarge
//768		1366	1.5		Android_XLarge

1200	1920		2		Android_XLarge
800		1280		1.5		Android_XLarge	
//768		1280		1.5		Android_XLarge
//800		1024		1		Android_XLarge
//768		1024		1		Android_XLarge
600		1024		1		Android_Large
							
640		960		2		Android_Normal
//540		960		2		Android_Normal
//480		854		1.5		Android_Normal
//600		800		1.5		Android_Normal
480		800			1.5		Android_Normal
//400		800			1.5		Android_Normal
320		480			1		Android_Normal

240		320			1		Android_Small
		
// Kindle Fire (3rd generation Covered by other x-large androids)

//2560	1600		2		KindleFireHDX_3rd_8_9
//1920	1200		2		KindleFireHDX_3rd_7
//1280	800			1.5		KindleFireHD_3rd_7

//1920	1200		1.5		KindleFireHD_2nd_8_9
//1280	800			1.5		KindleFireHD_2nd_7
//1024	600			1		KindleFire_2nd

//1024	600			1		KindleFire_1st
";
}

