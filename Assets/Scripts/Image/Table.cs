using UnityEngine;

public class Table
{
    [SerializeField] private int m_answersColumns = 4;
    [SerializeField] private int m_closedAnswersRows = 20;
    [SerializeField] private int m_openAnswersRows = 2;

    private int m_columns;
    private int m_rows;

    public Table() => SetTable();
    public Table(int closedAnswersRows, int openAnswersRows, int closedAnswersOptions) => SetTable(closedAnswersRows, openAnswersRows, closedAnswersOptions);

    public int AnswersColumns => m_answersColumns;
    public int ClosedAnswersRows => m_closedAnswersRows;
    public int OpenAnswersRows => m_openAnswersRows;
    public int Columns => m_columns;
    public int Rows => m_rows;

    public void SetTable(int closedAnswersRows, int openAnswersRows, int closedAnswersOptions)
    {
        m_openAnswersRows = openAnswersRows;

        if (closedAnswersOptions > 0)
        {
            m_answersColumns = closedAnswersOptions;
            m_columns = m_answersColumns + 1;
        }

        if (closedAnswersRows > 0)
        {
            m_closedAnswersRows = closedAnswersRows;
            m_rows = m_closedAnswersRows + m_openAnswersRows + 1;
        }
    }

    private void SetTable()
    {
        m_columns = m_answersColumns + 1;
        m_rows = m_closedAnswersRows + m_openAnswersRows + 1;
    }
}
