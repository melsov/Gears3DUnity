using UnityEngine;

public class Guid : MonoBehaviour
{
	[SerializeField]
	private string guidAsString;
	
	private System.Guid _guid;
	public System.Guid guid
	{
		get
		{
			if ( _guid == System.Guid.Empty &&
			    !System.String.IsNullOrEmpty( guidAsString ) )
			{
				_guid = new System.Guid( guidAsString );
			}
			
			return _guid;
		}
        set {
            _guid = value;
        }
	}
	
	public void Generate()
	{
		_guid = System.Guid.NewGuid();
		guidAsString = guid.ToString();
	}
}