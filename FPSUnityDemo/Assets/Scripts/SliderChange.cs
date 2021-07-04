using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class SliderChange : MonoBehaviour {
    [System.Serializable]
     public class OnValueChanged : UnityEvent<float> { };
     public OnValueChanged selectedEvent;
    Slider slider; // The slider this script is attached to
    //public UnityEvent<float> selectedEvent;

    public Text attachedValue; // The Text Label I want to change with the slider
	public void Awake(){
        Character player = GameObject.Find("Player").GetComponent<Character>();
		slider = GetComponent<Slider> (); // Grab the Slider
        // The line below ALL OF THESE COMMENTS is important: we tell the Slider that
        // whenever its value changes, it should automatically call the method
        // "OnSliderWasChanged".

        // slider = our slider
        
        // slider.onValueChanged = the "callback" executed when the slider's value 
        //     is changed.
        
        // slider.onValueChanged.AddListener(    ...    ); = adds a runtime callback,
        // or a "non-persistent listener" to the slider's "onValueChanged" callback.
        
        // ...(delegate {OnSliderWasChanged();}); = This is where we add the method
        // to call when the slider is changed. The method should be public and take 
        // no arguments.
        
        // The Method I used is named "OnSliderWasChanged", and it still functions as
        // a normal method (it's called below to automagically replace the 
        // placeholder).

        // MOAR INFO: https://docs.unity3d.com/ScriptReference/UI.Slider-onValueChanged.html
		slider.onValueChanged.AddListener (delegate {OnSliderWasChanged(); });
		OnSliderWasChanged();
	}

    // OnDisabled() & OnDestroy(): When the GameObject is not in use anymore, we
    // should use "slider.onValueChanged.RemoveAllListeners ();". This removes any
    // Listeners we added via code; such as our "OnSliderWasChanged" method.
	void OnDisabled(){    slider.onValueChanged.RemoveAllListeners ();    }
	void OnDestroy() {    slider.onValueChanged.RemoveAllListeners ();    }

    // These are methods to change the value of the slider programatically - this is 
    // more useful for healthbars, or sliders you have disabled.
	public void ChangeSlider(float h)   {    slider.value = h;     }
	public void incrementSlider(float h){    slider.value += h;    }
	public void decrementSlider(float h){    slider.value -= h;    }

    // This is the method that is called when the slider's value changes.
    public void OnSliderWasChanged(){
        selectedEvent.Invoke(slider.value);
        attachedValue.text = slider.value.ToString();
    }
}