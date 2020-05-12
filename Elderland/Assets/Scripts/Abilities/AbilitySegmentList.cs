using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySegmentList
{
	private AbilitySegment head;
	private AbilitySegment end;

	public AbilitySegment Start { get { return head.Next; } }

	public AbilitySegmentList()
	{
		head = new AbilitySegment(null);
		end = head;
	}

	public void AddSegment(AnimationClip clip, params AbilityProcess[] processes)
	{
		AbilitySegment temp = new AbilitySegment(clip, processes);
		end.Next = temp;
		end = temp;
	}

	public void AddSegment(AbilitySegment segment)
	{
		AbilitySegment temp = segment;
		end.Next = temp;
		end = temp;
	}

	public void NormalizeSegments()
	{
		AbilitySegment current = Start;
		while (current != null)
		{
			current.Normalize();
			current = current.Next;
		}
	}
}
