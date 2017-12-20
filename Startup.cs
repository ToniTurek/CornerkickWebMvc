using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CornerkickWebMvc.Startup))]
namespace CornerkickWebMvc
{
  public partial class Startup
  {
    public void Configuration(IAppBuilder app)
    {
        ConfigureAuth(app);
    }
  }
}
