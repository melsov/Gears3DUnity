using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pause : MonoBehaviour {

    public Button pauseButton;
    public Sprite pausedSprite;
    public Sprite unpausedSprite;

    public bool paused {
        get { return _paused; }
    }
    private bool _paused;
    
    void Awake() {
        setImage();
    }

    public void pausePressed() {
        _paused = !_paused;
        setImage();
        Time.timeScale = _paused ? 0f : 1f;
    }

    private void setImage() {
        pauseButton.GetComponent<Image>().sprite = _paused ? pausedSprite : unpausedSprite;
    }

}
