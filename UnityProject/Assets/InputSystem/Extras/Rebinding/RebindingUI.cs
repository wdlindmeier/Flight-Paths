using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.Input;
using UnityEngine.UI;

public class RebindingUI : MonoBehaviour
{
    public Button controlSchemeButtonTemplate;
    public BindingUIElement bindingUIElementTemplate;
    public Button doneButton;
    public Button resetButton;

    // Scope is action map.
    ActionMapInput m_ActionMapInput;
    PlayerHandle m_PlayerHandle;

    // Scope is control scheme.
    int m_ControlSchemeIndex;
    List<Button> m_ControlSchemeButtons = new List<Button>();
    List<BindingUIElement> m_BindingUIElements = new List<BindingUIElement>();
    List<LabeledBinding> m_Bindings = new List<LabeledBinding>();
    IEndBinding m_BindingToBeAssigned;

    public void Awake()
    {
        doneButton.onClick.AddListener(DeactivateUI);
        resetButton.onClick.AddListener(ResetActiveControlScheme);
    }

    public void Initialize(ActionMapInput actionMapInput, PlayerHandle playerHandle)
    {
        m_ActionMapInput = actionMapInput;
        m_PlayerHandle = playerHandle;

        if (m_ActionMapInput == null || m_PlayerHandle == null)
            return;

        InitializeControlScheme();
        ActivateUI();
    }

    void InitializeControlScheme()
    {
        var devices = m_PlayerHandle.GetApplicableDevices();
        m_ActionMapInput.TryInitializeWithDevices(devices, null, m_ControlSchemeIndex);
        m_Bindings.Clear();
        m_ActionMapInput.controlSchemes[m_ControlSchemeIndex].ExtractLabeledEndBindings(m_Bindings);
    }

    void ActivateUI()
    {
        gameObject.SetActive(true);

        controlSchemeButtonTemplate.gameObject.SetActive(true);
        bindingUIElementTemplate.gameObject.SetActive(true);

        ActivateControlSchemeButtons();
        ActivateBindingUIElements();

        controlSchemeButtonTemplate.gameObject.SetActive(false);
        bindingUIElementTemplate.gameObject.SetActive(false);
    }

    void DeactivateUI()
    {
        gameObject.SetActive(false);
    }

    bool BindInputControl(InputControl control)
    {
        if (!m_ActionMapInput.BindControl(m_BindingToBeAssigned, control, true))
            return false;

        m_BindingToBeAssigned = null;
        return true;
    }

    bool IsNewBindingAssigned(LabeledBinding labeledBinding)
    {
        if (labeledBinding.binding != m_BindingToBeAssigned)
            return true;
        else
            return false;
    }

    void ActivateControlSchemeButtons()
    {
        if (m_ControlSchemeButtons.Count < m_ActionMapInput.controlSchemes.Count)
        {
            for (int i = m_ControlSchemeButtons.Count; i < m_ActionMapInput.controlSchemes.Count; i++)
            {
                var newControlSchemeButton = Instantiate(controlSchemeButtonTemplate, controlSchemeButtonTemplate.transform.parent, false);
                m_ControlSchemeButtons.Add(newControlSchemeButton);
            }
        }
        else if (m_ControlSchemeButtons.Count > m_ActionMapInput.controlSchemes.Count)
        {
            for (int i = m_ControlSchemeButtons.Count; i > m_ActionMapInput.controlSchemes.Count; i--)
            {
                Destroy(m_ControlSchemeButtons[i - 1]);
                m_ControlSchemeButtons.RemoveAt(i - 1);
            }
        }

        for (int i = 0; i < m_ActionMapInput.controlSchemes.Count; i++)
        {
            Button button = m_ControlSchemeButtons[i];

            if (m_ControlSchemeIndex == i)
            {
                button.interactable = false;
            }
            else if (button.interactable == false)
            {
                button.interactable = true;
            }

            int tempInt = i;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { ChangeControlSchemeIndex(tempInt); });

            Text label = button.GetComponentInChildren<Text>();
            label.text = m_ActionMapInput.controlSchemes[i].name;
        }
    }

    void ActivateBindingUIElements()
    {
        ControlScheme scheme = m_ActionMapInput.controlSchemes[m_ControlSchemeIndex];

        if (m_BindingUIElements.Count < m_Bindings.Count)
        {
            for (int i = m_BindingUIElements.Count; i < m_Bindings.Count; i++)
            {
                var newBindingUIElement = Instantiate(bindingUIElementTemplate, bindingUIElementTemplate.transform.parent, false);
                m_BindingUIElements.Add(newBindingUIElement);
            }
        }
        else if (m_BindingUIElements.Count > m_Bindings.Count)
        {
            for (int i = m_BindingUIElements.Count - 1; i >= m_Bindings.Count; i--)
            {
                Destroy(m_BindingUIElements[i].gameObject);
                m_BindingUIElements.RemoveAt(i);
            }
        }

        for (int i = 0; i < m_Bindings.Count; i++)
        {
            ActivateBindingUIElement(i, m_Bindings[i], scheme);
        }
    }

    void ActivateBindingUIElement(int i, LabeledBinding labeledBinding, ControlScheme scheme)
    {
        BindingUIElement element = m_BindingUIElements[i];
        element.actionText.text = labeledBinding.label;

        if (labeledBinding.binding == m_BindingToBeAssigned || labeledBinding.binding == null)
        {
            return;
        }
        else
        {
            element.bindingText.text = labeledBinding.binding.GetSourceName(scheme, false);
            Button button = element.bindingButton;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { ChangeBinding(labeledBinding, scheme, element.bindingText); });
        }
    }

    void ChangeControlSchemeIndex(int i)
    {
        if (i == m_ControlSchemeIndex)
            return;

        m_ControlSchemeIndex = i;
        Initialize(m_ActionMapInput, m_PlayerHandle);
    }

    void ChangeBinding(LabeledBinding labeledBinding, ControlScheme scheme, Text label)
    {
        m_BindingToBeAssigned = labeledBinding.binding;
        InputSystem.ListenForBinding(BindInputControl);
        label.text = "...";
        StartCoroutine(BindingLabelUpdater(label, labeledBinding, scheme));
    }

    IEnumerator BindingLabelUpdater(Text label, LabeledBinding labeledBinding, ControlScheme scheme)
    {
        yield return new WaitUntil(() => IsNewBindingAssigned(labeledBinding));

        label.text = labeledBinding.binding.GetSourceName(scheme, false);
    }

    public void ResetActiveControlScheme()
    {
        List<ControlScheme> currentControlSchemes = m_ActionMapInput.controlSchemes;
        m_ActionMapInput.ResetControlSchemes();

        for (int i = 0; i < m_ActionMapInput.controlSchemes.Count; i++)
        {
            if (i != m_ControlSchemeIndex)
                m_ActionMapInput.controlSchemes[i] = currentControlSchemes[i];
        }

        Initialize(m_ActionMapInput, m_PlayerHandle);
    }
}
