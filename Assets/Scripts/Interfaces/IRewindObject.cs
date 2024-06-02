using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RewindProject
{
    public interface IRewindObject
    {
        public IRewindController Controller { get; }
        /// <returns>������ ��������, �������� event-based �������� (� ������).</returns>
        public int GetSizeOfEventProps();
        /// <summary>
        /// ������� ������� �� state-based ��� � ����������.
        /// </summary>
        public void Tick(in int frameID);
        /// <summary>
        /// ������������� ��������� IRewindableComponent'��, ���������� IsMainThreadComponent == true.
        /// </summary>
        public void SetFrameToMainThreadComponents(in int frameID);
        /// <summary>
        /// ������������� rewindable-���������� � ��������� ���������� �����.
        /// </summary>
        /// <param name="frameID">ID ����� �� ���� �����o������ FixedUpdate() (�������� �����������).</param>
        public void SetFrame(in int frameID);
        /// <summary>
        /// ������� ���������� � ������, ���������� ����� ������������� frameID.
        /// </summary>
        public void ClearInfo(in int frameID);
        /// <summary>
        /// �������� ������ ���������� ����������� �������.
        /// </summary>
        public void EnablePhysics();
        /// <summary>
        /// ��������� ���������� ���������� �������.
        /// </summary>
        public void DisablePhysics();
    }
}