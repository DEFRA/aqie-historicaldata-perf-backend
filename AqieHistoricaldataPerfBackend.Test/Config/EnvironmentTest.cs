using Microsoft.AspNetCore.Builder;

namespace AqieHistoricaldataPerfBackend.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   { 
       var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
       var isDev = AqieHistoricaldataPerfBackend.Config.Environment.IsDevMode(builder);
       Assert.False(isDev);
   }
}
