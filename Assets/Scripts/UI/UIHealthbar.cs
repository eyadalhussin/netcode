using Unity.Netcode;
using UnityEngine.UI;

public class UIHealthbar : NetworkBehaviour
{
    private NetworkVariable<float> _sliderValue = new(0f);

    public Slider _slider;
    private float _currentHealth;
    LifeComponent lf;

    // Update is called once per frame

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _sliderValue.OnValueChanged += OnHealthBarSliderChanged;
    }
    void Start()
    {
        lf = GetComponent<LifeComponent>();
    }
    void Update()
    {
        if (!IsServer) return;

        if (lf && _slider)
        {
            _currentHealth = lf.GetCurrentHealth();
            if (_sliderValue.Value != _currentHealth)
            {
                UpdateSliderValue(_currentHealth);
            }
        }
    }

    private void UpdateSliderValue(float newValue)
    {
        _sliderValue.Value = newValue;
    }

    private void OnHealthBarSliderChanged(float oldValue, float newValue)
    {
        if (_slider != null)
        {
            _slider.value = newValue;
        }
    }

}
