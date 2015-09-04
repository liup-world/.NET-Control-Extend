
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;

[assembly: TagPrefix("BobSystem.Controls", "mng")]
namespace BobSystem.Controls
{
    /// <summary>
    /// 可编辑的 下拉列表框
    /// </summary>
    public class DropDownListX
        : CompositeControl
    {
        /*
         *  大体逻辑是
         *  1. 包装一个DropDownList
         *  2. 创建一个TextBox，覆盖到DropDownList之上
         *  只完成原型，架子起来了 没定制功能
         */
        public DropDownListX()
        {
            this.Width = Unit.Pixel(150);
            this.Height = Unit.Pixel(21);
        }

        #region FIELD and PROPERTY

        private TextBox txt = new TextBox();
        private DropDownList ddl = new DropDownList();

        [DefaultValue("150px"), Description("控件的宽度。")]
        public override Unit Width
        {
            get { return base.Width; }
            set { base.Width = value; }
        }

        [DefaultValue("21px"), Description("控件的宽度。")]
        public override Unit Height
        {
            get { return base.Height; }
            set { base.Height = value; }
        }

        #endregion

        #region Drop Down List Property

        public object DataSource
        {
            get { return this.ddl.DataSource; }
            set { this.ddl.DataSource = value; }
        }

        public new void DataBind()
        {
            this.ddl.DataBind();
        }

        public ListItem SelectedItem
        {
            get { return this.ddl.SelectedItem; }
        }

        [DefaultValue(0), Description("选择的项的索引。")]
        public int SelectedIndex
        {
            get { return this.ddl.SelectedIndex; }
            set { this.ddl.SelectedIndex = value; }
        }

        public string SelectedValue
        {
            get { return this.ddl.SelectedValue; }
            set { this.ddl.SelectedValue = value; }
        }
        #endregion

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            int dropDownBtnWidth = 23;

            int txtWidth = (int)this.Width.Value - dropDownBtnWidth;
            this.txt.Width = Unit.Pixel(txtWidth);
            this.txt.Height = Unit.Pixel((int)this.Height.Value - 6);
            this.txt.Attributes.Add("style", "position:absolute; z-index:1;");
            this.txt.Attributes.Add("ondblclick", "textBox_ondblclick(this)");

            this.ddl.Width = this.Width;
            this.ddl.Height = this.Height;
            this.ddl.Attributes.Add("onchange", "dropDownList_onchange(this)");
            //this.ddl.Items.Add(new ListItem("A", "a"));
            //this.ddl.Items.Add(new ListItem("B", "b"));
            //this.ddl.Items.Add(new ListItem("C", "c"));
            //this.ddl.Items.Add(new ListItem("D", "d"));

            base.Controls.Add(this.txt);
            base.Controls.Add(this.ddl);

            base.Page.ClientScript.RegisterClientScriptResource(GetType(), "BobSystem.Controls.Scripts.DropDownListX_required.js");
        }
    }
}
