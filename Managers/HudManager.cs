using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    // Singleton instance
    public static HudManager Instance;
    
    [SerializeField] 
    private TMP_Text InteractText;

    //todo change "Interact" to be context sensitive
    private string InteractStringPrefix = "Press F to Interact with ";
    
    // Start is called before the first frame update
    void Start()
    {
        if (Instance)
        {
            Debug.LogError("Trying to instantiate a second InteractableManager.");
            Destroy(this);
        }
        Instance = this;
    }

    public void ShowInteractText(bool bShowText)
    {
        InteractText.enabled = bShowText;
    }
    
    public void ShowAndUpdateInteractText(string InteractableName, bool bShowText)
    {
       // switch (expression) // Todo: change text based on state
        {
            
        }
        
        InteractText.text = InteractStringPrefix + InteractableName;
        InteractText.enabled = bShowText;
    }
}
