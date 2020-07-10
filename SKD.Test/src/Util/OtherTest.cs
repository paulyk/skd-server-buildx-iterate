using System;
using System.Collections.Generic;
using SKD.Model;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SKD.Test {
  public class OtherTest {

    [Fact]
    public void entity_trim_all_string_properties_works() {
      var codeValue = "Code ";
      var nameValue = "Name  ";
      var component = new Component {
        Code = codeValue,
        Name = nameValue
      };

      Assert.Equal(component.Code.Length, codeValue.Length);
      Assert.Equal(component.Name.Length, nameValue.Length);

      component.TrimStringProperties();

      Assert.Equal(component.Code.Length, codeValue.Trim().Length);
      Assert.Equal(component.Name.Length, nameValue.Trim().Length);
    }
  }
}