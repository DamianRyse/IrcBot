using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrcBot
{
    class toolbox
    {
        /// <summary>
        /// Vergrößert das angegebene Array vom Typ T um eine bestimmte Größe und gibt es zurück.
        /// </summary>
        /// <typeparam name="T">Der Typ des Arrays</typeparam>
        /// <param name="Arr">Das Array, welches vergrößert werden soll.</param>
        /// <param name="AddElements">Die Anzahl an zusätzlichen Elementen, die hinzugefügt werden sollen.</param>
        /// <returns>Vergrößertes Array vom Typ T</returns>
        public static T[] ResizeArray<T>(T[] Arr, int AddElements)
        {
            // Prüfen, ob das alte Array null ist. Wenn nicht, dann addiere AddElements auf die Größe des alten Arrays auf
            // und übertrage alle Elemente vom alten Array auf newArray.
            T[] newArray;
            if (Arr != null)
            {
                newArray = new T[Arr.Length + AddElements];
                for (int i = 0; i < Arr.Length; i++)
                {
                    newArray[i] = Arr[i];
                }
            }
            else
            {
                newArray = new T[1];
            }
            return newArray;
        }
    }
}
