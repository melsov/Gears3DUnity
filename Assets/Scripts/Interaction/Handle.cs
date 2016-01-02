using UnityEngine;
using System.Collections;
using System;

public class Handle : MonoBehaviour  {

    public Transform widget;

    public delegate bool IsCursorInteracting();
    public IsCursorInteracting dIsCursorInteracting;
}
