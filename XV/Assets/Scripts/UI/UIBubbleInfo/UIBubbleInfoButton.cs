using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class UIBubbleInfoButton
{

	public string Text { get; set; }

	public Action<ObjectEntity> ClickAction { get; set; }

}
