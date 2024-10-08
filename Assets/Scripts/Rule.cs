using System;
using Unity.Collections;

[Serializable]
public struct Rule
{
    public NativeArray<bool> surviveRules; // Правила выживания
    public NativeArray<bool> bornRules;    // Правила рождения

    /// <summary>
    /// Конструктор для инициализации правил.
    /// </summary>
    /// <param name="survive">Массив чисел соседей для выживания.</param>
    /// <param name="born">Массив чисел соседей для рождения.</param>
    /// <param name="allocator">Аллокатор для NativeArray.</param>
    public Rule(int[] survive, int[] born, Allocator allocator)
    {
        surviveRules = new NativeArray<bool>(9, allocator);
        bornRules = new NativeArray<bool>(9, allocator);

        // Инициализация всех правил как false
        for (int i = 0; i < 9; i++)
        {
            surviveRules[i] = false;
            bornRules[i] = false;
        }

        // Установка правил выживания
        foreach (var s in survive)
        {
            if (s >= 0 && s < 9)
                surviveRules[s] = true;
        }

        // Установка правил рождения
        foreach (var b in born)
        {
            if (b >= 0 && b < 9)
                bornRules[b] = true;
        }
    }

    /// <summary>
    /// Освобождает ресурсы NativeArray.
    /// </summary>
    public void Dispose()
    {
        if (surviveRules.IsCreated)
            surviveRules.Dispose();
        if (bornRules.IsCreated)
            bornRules.Dispose();
    }
}