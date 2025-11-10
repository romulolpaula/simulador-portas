using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class InputNode : GateModel
    {
        private bool _value;

        // Valor lógico da entrada
        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnValueChanged?.Invoke(); // Dispara evento sempre que muda
                }
            }
        }

        public event Action? OnValueChanged;

        // Método auxiliar (opcional, se quiser inverter com clique)
        public void Toggle()
        {
            Value = !Value;
        }

        // Define o comportamento lógico da entrada
        protected override bool ComputeOutput()
        {
            return Value;
        }

        // Recalcula saída explicitamente quando for chamado
        public override bool Evaluate(HashSet<Guid> visited)
        {
            Output = Value;
            return Output;
        }


    }
}
