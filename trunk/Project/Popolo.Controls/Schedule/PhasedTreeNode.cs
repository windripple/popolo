/* PhasedTreeNode.cs
 * 
 * Copyright (C) 2007 E.Togashi
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or (at
 * your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

namespace Popolo.Schedule
{
    /// <summary>�q�m�[�h�̒ǉ�������ۑ�����v���p�e�B�����i�K�I�c���[�m�[�h</summary>
    public class PhasedTreeNode : System.Windows.Forms.TreeNode
    {

        /// <summary>�q�m�[�h�ǉ������ς��ۂ��̃t���O</summary>
        private bool isSubFoldersAdded = false;

        /// <summary>�R���X�g���N�^</summary>
        /// <param name="text">�c���[�m�[�h�ɕ\�����镶����</param>
        public PhasedTreeNode(string text) : base(text)
        {
            isSubFoldersAdded = false;
        }

        /// <summary>�q�m�[�h�ǉ������ς��ۂ��̃t���O��ݒ�E�擾����</summary>
        public bool SubFoldersAdded
        {
            get
            {
                return isSubFoldersAdded;
            }
            set
            {
                isSubFoldersAdded = value;
            }
        }

    }
}
