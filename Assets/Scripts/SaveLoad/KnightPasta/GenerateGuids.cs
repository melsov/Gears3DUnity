using UnityEngine;
using UnityEditor;

public class GenerateGuids : ScriptableWizard
{
    //No need for this
	//[MenuItem( "Custom/Re-generate Guids" )]
	//static void Generate()
	//{
	//	GameObject[] objList = GameObject.FindObjectsOfType( typeof( GameObject ) ) as GameObject[];
	//	foreach ( GameObject obj in objList )
	//	{
	//		Guid guidObj = obj.GetComponent<Guid>();
	//		if ( guidObj != null )
	//		{
	//			guidObj.Generate();
				
	//			GUI.changed = true;
	//			EditorUtility.SetDirty( guidObj );
	//		}
	//	}
	//}
}