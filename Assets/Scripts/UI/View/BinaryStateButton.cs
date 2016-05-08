using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BinaryStateButton : MonoBehaviour {
    [SerializeField]
    protected Sprite onStateSprite;
    [SerializeField]
    protected Sprite offStateSprite;
    protected Button button;

    public delegate void PressAction();

    [SerializeField]
    public MonoBehaviour bSBClient;
    protected IBinaryStateButtonClient client;

    public void Awake() {
        client = bSBClient.GetComponent<IBinaryStateButtonClient>();
        button = GetComponent<Button>();
    }

    public void toggleState() {
        client.getPressAction()();
        button.image.sprite = client.getState() ? onStateSprite : offStateSprite;
    }

}

public interface IBinaryStateButtonClient
{
    bool getState();
    BinaryStateButton.PressAction getPressAction();
}


