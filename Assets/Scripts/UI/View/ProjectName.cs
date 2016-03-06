using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class ProjectName : MonoBehaviour {

    private InputField projectNameInput;

	void Start () {
        projectNameInput = gameObject.transform.parent.GetComponentInChildren<InputField>();
        if (projectNameInput.text == "") {
            projectNameInput.text = projectNameInput.placeholder.GetComponent<Text>().text + randomLetters();
        }
        Browser.Instance.projectName = projectNameInput.text;
        projectNameInput.onEndEdit.AddListener(handleEndEdit);

        Browser.Instance.handleNewProjectName = browserGotNewProjectName;
	}

    private string randomLetters() {
        string result = projectNameInput.text;
        int a = 'a';
        for (int i = 0; i < 3; ++i) {
            int r = Random.Range(0, 26) + a;
            result += ((char)r);
        }

        return result;
    }

    public void browserGotNewProjectName(string _newName) {
        projectNameInput.text = _newName;
    }

    public void handleEndEdit(string _name) {
        Debug.LogError("new name: " + _name);
        Browser.Instance.projectName = _name;
    }


}
