using UnityEngine;
using UnityEngine . UI;

[RequireComponent ( typeof ( Slider ) )]
public class SliderHandleNumberController : MonoBehaviour
{
    [SerializeField]
    private Slider _slide;
    [SerializeField]
    private Text _text;

    // Use this for initialization
    void Start ( )
    {
        OnChangeValue ( );
    }

    public void OnChangeValue ( )
    {
        this . _text . text = this . _slide . value . ToString ( );
    }
}
