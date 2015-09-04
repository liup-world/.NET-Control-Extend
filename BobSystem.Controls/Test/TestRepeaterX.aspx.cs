using System;
using System.Data;
using BobSystem.Controls;

namespace Test
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsPostBack)
            {
                bindData();
                bindData2();
                bindData3();
            }
        }

        private void bindData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("aa", typeof(string));
            dt.Columns.Add("bb", typeof(string));
            dt.Columns.Add("cc", typeof(string));
            dt.Columns.Add("dd", typeof(string));
            dt.Columns.Add("ee", typeof(string));
            dt.Columns.Add("ff", typeof(string));

            for (int i = 1; i < 100; i++)
            {
                string str = i.ToString("00");
                dt.Rows.Add(new object[] { str, str, str, str, str, str });
            }

            rpt.DataSource = dt;
            rpt.DataBind();
        }

        private void bindData2()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("aa", typeof(string));
            dt.Columns.Add("bb", typeof(string));
            dt.Columns.Add("cc", typeof(string));
            dt.Columns.Add("dd", typeof(string));
            dt.Columns.Add("ee", typeof(string));

            for (int i = 101; i < 200; i++)
            {
                string str = i.ToString("000");
                dt.Rows.Add(new object[] { str, str, str, str, str });
            }

            rpt2.DataSource = dt;
            rpt2.DataBind();
        }

        private void bindData3()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("aa", typeof(string));
            dt.Columns.Add("bb", typeof(string));
            dt.Columns.Add("cc", typeof(string));
            dt.Columns.Add("dd", typeof(string));
            dt.Columns.Add("ee", typeof(string));

            for (int i = 201; i < 300; i++)
            {
                string str = i.ToString("000");
                dt.Rows.Add(new object[] { str, str, str, str, str });
            }
            rpt3.DataSource = dt;
            rpt3.DataBind();
        }

        protected void rpt_PageIndexChanging(object sender, RepeaterPageEventArgs e)
        {
            rpt.PageIndex = e.NewPageIndex;
            bindData();
        }

        protected void rpt2_PageIndexChanging(object sender, RepeaterPageEventArgs e)
        {
            rpt2.PageIndex = e.NewPageIndex;
            bindData2();
        }

        protected void rpt3_PageIndexChanging(object sender, RepeaterPageEventArgs e)
        {
            rpt3.PageIndex = e.NewPageIndex;
            bindData3();
        }
    }
}
