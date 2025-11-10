using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// KeypadPanel: Panel principal que gestiona la lógica del teclado numérico.
/// </summary>
public class KeypadPanel : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] string correctCode = "1234"; //codigo correcto
    [SerializeField] MeshRenderer panelRenderer; //material del panel
    [SerializeField] TMP_Text displayMesh; //texto que muestra los dígitos
    [SerializeField] GameObject tickCorrect; //tick verde
    [SerializeField] GameObject tickWrong; //X roja
    [SerializeField] InjectionDoorController linkedDoor; //referencia a la puerta

    string currentInput = "";

    //Recive el número de los botones
    public void ReceiveInput(int number)
    {
        if (currentInput.Length >= correctCode.Length) return;

        currentInput += number.ToString();
        UpdateDisplay();

        if (currentInput.Length >= correctCode.Length)
            CheckCode();
    }

    //Borra el último dígito introducido
    public void DeleteLastDigit()
    {
        if (currentInput.Length == 0) return;

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        displayMesh.text = currentInput;
    }

    void CheckCode()
    {
        bool success = currentInput == correctCode;
        StartCoroutine(Feedback(success));
    }

    IEnumerator Feedback(bool success)
    {
        panelRenderer.material.color = success ? Color.green : Color.red;
        if (success)
        {
            tickCorrect.SetActive(true);
            AudioManager.Instance.Play("CorrectAnswer");
        }
        if (!success)
        {
            tickWrong.SetActive(false);
            AudioManager.Instance.Play("WrongAnswer");
        }

        if(success && linkedDoor != null)
            linkedDoor.OpenDoor();

        //Si falla, borrar todo el input
        if (!success)
        {
            currentInput = "";
        }
        UpdateDisplay();

        yield return new WaitForSeconds(1.5f);

        panelRenderer.material.color = Color.white;
        tickCorrect.SetActive(false);
        tickWrong.SetActive(false);

        //Reset total del input
        if (success) currentInput = "";
        UpdateDisplay();
    }
}
