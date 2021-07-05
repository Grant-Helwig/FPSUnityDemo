using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class SliderChange : MonoBehaviour {
    [System.Serializable]
    public class OnValueChanged : UnityEvent<float> { };
    public OnValueChanged selectedEvent;
    public OnValueChanged defaultVal;
    Slider slider; // The slider this script is attached to

    public Text attachedValue; // The Text Label I want to change with the slider

    public void Awake(){
        Character player = GameObject.Find("Player").GetComponent<Character>();
		slider = GetComponent<Slider> (); // Grab the Slider

		slider.onValueChanged.AddListener (delegate {OnSliderWasChanged(); });
		OnSliderWasChanged();
	}

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