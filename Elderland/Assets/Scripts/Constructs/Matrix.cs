using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A matrix structure for solving systems of linear equations.
public class Matrix 
{
	private float[,] backing;
	private int rows;
	private int columns;

	public Matrix(int rows, int columns)
	{
		backing = new float[rows,columns];
		this.rows = rows;
		this.columns = columns;
	}

	public float GetEntry(int row, int column)
	{
		return backing[row, column];
	}

	public void SetColumn(int c, float[] column)
	{
		for (int i = 0; i < rows; i++)
		{
			backing[i, c] = column[i];
		}
	}

	public void SetRow(int r, float[] row)
	{
		for (int i = 0; i < columns; i++)
		{
			backing[r, i] = row[i];
		}
	}

	public float[] GetColumn(int c)
	{
		float[] column = new float[rows];
		for (int i = 0; i < rows; i++)
		{
			column[i] = backing[i, c];
		}
		return column;
	}

	public float[] GetRow(int r)
	{
		float[] row = new float[columns];
		for (int i = 0; i < columns; i++)
		{
			row[i] = backing[r, i];
		}
		return row;
	}

	public float[] Multiply(float[] entry, float m)
	{
		float[] newEntry = new float[entry.Length];
		for (int i = 0; i < entry.Length; i++)
		{
			newEntry[i] = m * entry[i];
		}
		return newEntry;
	}

	public float[] Add(float[] entry1, float[] entry2)
	{
		float[] newEntry = new float[entry1.Length];
		for (int i = 0; i < entry1.Length; i++)
		{
			newEntry[i] = entry1[i] + entry2[i];
		}
		return newEntry;
	}

	public void SwapRows(int row1, int row2)
	{
		float[] temp = GetRow(row1);
		SetRow(row1, GetRow(row2));
		SetRow(row2, temp);
	}

	public void Print()
	{
		for (int i = 0; i < rows; i++)
		{
			string row = "";
			for (int j = 0; j < columns - 1; j++)
			{
				row += backing[i, j];
				row += ", ";
			}
			row += backing[i, columns - 1];
			Debug.Log(row);
		}
	}

	public Matrix ToRREF(float solutionMargin = 0)
	{
		if (columns < rows)
		{
			throw new System.InvalidOperationException("Invalid Dimensions, more rows than column");
		}
		else
		{
			Matrix reduced = new Matrix(rows, columns);
			for (int i = 0; i < columns; i++)
			{
				reduced.SetColumn(i, GetColumn(i));
			}

			//Solve
			int x = 0;
			int y = 0;
			while (x < columns && y < rows)
			{
				if (reduced.backing[y, x] != 0)
				{
					reduced.SetRow(y, reduced.Multiply(reduced.GetRow(y), 1 / reduced.backing[y, x]));
				
					for (int j = 0; j < rows; j++)
					{
						if (j != y)
						{
							float[] newRow = reduced.GetRow(j);
							newRow = reduced.Add(newRow, reduced.Multiply(reduced.GetRow(y), -reduced.backing[j, x]));
							reduced.SetRow(j, newRow);
						}
					}

					y++;
				}
				else
				{
					//Search for non zero term in column
					bool foundNonZero = false;
					int nonZeroRow = 0;
					for (int i = y + 1; i < rows; i++)
					{
						if (reduced.backing[i, x] != 0)
						{
							foundNonZero = true;
							nonZeroRow = i;
							break;
						}
					}

					if (foundNonZero)
					{
						reduced.SwapRows(y, nonZeroRow);
						reduced.SetRow(y, reduced.Multiply(reduced.GetRow(y), 1 / reduced.backing[y, x]));
				
						for (int j = 0; j < rows; j++)
						{
							if (j != y)
							{
								float[] newRow = reduced.GetRow(j);
								newRow = reduced.Add(newRow, reduced.Multiply(reduced.GetRow(y), -reduced.backing[j, x]));
								reduced.SetRow(j, newRow);
							}
						}

						y++;
					}
				}
				x++;
			}

			return reduced;
		}
	}
}