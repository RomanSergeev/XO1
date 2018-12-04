using System;
using System.Web.Mvc;
using XO1.Models;
using XO1.Services;

namespace XO1.Controllers
{
    public class GameController : Controller
    {
        GameContext gc = new GameContext();
        GameService gs = new GameService();
        // where and how to CREATE model object?
        // is game a model object?
        //private static XO1.Models.Game game = new Models.Game(3, 3, 3, Models.Game.STATE.O);

        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(int? fictive)
        {
            byte lines = Byte.Parse(Request.Form["lines"]);
            byte cols = Byte.Parse(Request.Form["cols"]);
            byte winLength = Byte.Parse(Request.Form["winLength"]);
            bool playerMoveFirst = Request.Form["playerMoveFirst"] == "on";
            Game game = new Game(lines, cols, winLength, playerMoveFirst);
            gc.Games.Add(game);
            gs.setup(game);
            return View(game);
        }
    }
}