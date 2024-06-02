using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RewindProject
{
    public interface IRewindObject
    {
        public IRewindController Controller { get; }
        /// <returns>Размер словарей, хранящих event-based свойства (в байтах).</returns>
        public int GetSizeOfEventProps();
        /// <summary>
        /// Передаёт команду на state-based тик в компоненты.
        /// </summary>
        public void Tick(in int frameID);
        /// <summary>
        /// Устанавливает состояния IRewindableComponent'ов, отмеченных IsMainThreadComponent == true.
        /// </summary>
        public void SetFrameToMainThreadComponents(in int frameID);
        /// <summary>
        /// Устанавливает rewindable-компоненты в состояние выбранного кадра.
        /// </summary>
        /// <param name="frameID">ID кадра из всех произoшедших FixedUpdate() (учитывая пропущенные).</param>
        public void SetFrame(in int frameID);
        /// <summary>
        /// Очищает информацию о кадрах, записанных после передаваемого frameID.
        /// </summary>
        public void ClearInfo(in int frameID);
        /// <summary>
        /// Включает работу физических компонентов объекта.
        /// </summary>
        public void EnablePhysics();
        /// <summary>
        /// Отключает физические компоненты объекта.
        /// </summary>
        public void DisablePhysics();
    }
}