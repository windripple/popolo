/* ITermStructure.cs
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

using System;

using System.Runtime.Serialization;

namespace Popolo.Schedule
{
    /// <remarks>季節や時間帯等の期間構造を持つインターフェース</remarks>
    public interface ITermStructure : ICloneable, ImmutableITermStructure
    {

        /// <summary>ItermStructureのIDを取得・設定する</summary>
        new int ID { get; set; }

        /// <summary>ITermStructureの名称を取得・設定する</summary>
        new string Name { get; set; }

    }

    /// <summary>読み取り専用期間構造インターフェース</summary>
    public interface ImmutableITermStructure
    {
        /// <summary>ItermStructureのIDを取得する</summary>
        int ID { get; }

        /// <summary>ITermStructureの名称を取得する</summary>
        string Name { get; }

        /// <summary>季節や時間帯等の期間名称リストを取得する</summary>
        /// <returns>季節や時間帯等の期間名称リスト</returns>
        string[] GetTermNames();

        /// <summary>日時を指定して季節や時間帯等の期間名称を取得する</summary>
        /// <param name="dateTime">日時</param>
        /// <returns>季節や時間帯等の期間名称</returns>
        string GetTermName(DateTime dateTime);
    }

}
