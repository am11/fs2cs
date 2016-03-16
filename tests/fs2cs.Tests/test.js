(function (global, factory) {
  if (typeof define === "function" && define.amd) {
    define(["exports", "./fable-core.js"], factory);
  } else if (typeof exports !== "undefined") {
    factory(exports, require("./fable-core.js"));
  } else {
    var mod = {
      exports: {}
    };
    factory(mod.exports, global.fableCore);
    global.unknown = mod.exports;
  }
})(this, function (exports, _fableCore) {
  "use strict";

  Object.defineProperty(exports, "__esModule", {
    value: true
  });

  var $M1 = _interopRequireWildcard(_fableCore);

  function _interopRequireWildcard(obj) {
    if (obj && obj.__esModule) {
      return obj;
    } else {
      var newObj = {};

      if (obj != null) {
        for (var key in obj) {
          if (Object.prototype.hasOwnProperty.call(obj, key)) newObj[key] = obj[key];
        }
      }

      newObj.default = obj;
      return newObj;
    }
  }

  exports.default = function (Test) {
    var SimpleArithmetic = Test.SimpleArithmetic = function () {
      var x, y, patternInput, r2, r1;
      return x = 10 + 12 - 3, y = x * 2 + 1, patternInput = [x / 3, x % 3], r2 = patternInput[1], r1 = patternInput[0], function () {
        var clo1;
        return clo1 = $M1.String.fsFormat("x = %d, y = %d, x/3 = %d, x%%3 = %d\n")(function (x) {
          console.log(x);
        }), function (arg10) {
          var clo2;
          return clo2 = clo1(arg10), function (arg20) {
            var clo3;
            return clo3 = clo2(arg20), function (arg30) {
              var clo4;
              return clo4 = clo3(arg30), function (arg40) {
                clo4(arg40);
              };
            };
          };
        };
      }()(x)(y)(r1)(r2);
    };

    return Test;
  }({});
});