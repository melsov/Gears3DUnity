using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pause : Singleton<Pause> {

    public Button pauseButton;
    public Sprite pausedSprite;
    public Sprite unpausedSprite;
    public delegate void OnPause(bool isPaused);
    public OnPause onPause;


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
        print("pause: " + _paused);
        onPause(_paused);
        //Time.timeScale = _paused ? 0f : 1f;
    }

    private void setImage() {
        pauseButton.GetComponent<Image>().sprite = _paused ? pausedSprite : unpausedSprite;
    }

}
