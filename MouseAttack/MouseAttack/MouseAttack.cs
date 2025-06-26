using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace MouseAttack;

/// @author roosa
/// @version 11.06.2025
/// <summary>
/// Luodaan peli
/// </summary>
public class MouseAttack : PhysicsGame
{
    private readonly Vector nopeusVasen = new Vector(-800, 0);
    private readonly Vector nopeusOikea = new Vector(800, 0);
    private double nopeusHiiri = -100;
    private Image tykkikuva = LoadImage("TykkiKuva.png");
    private Image karkkikasa = LoadImage("Karkkikasa.png");
    private Image[] hiirikuva = LoadImages("HiiriKuva1", "HiiriKuva2", "HiiriKuva3", "HiiriKuva4");
    private Image taustakuva = LoadImage("Taustakuva.png");
    private Image etukuva = LoadImage("Saippuakupla.png");
    private Timer ajastin;
    private ScoreList topLista = new ScoreList(10, false, 0);
    
    public override void Begin()
    {
        SetWindowSize(750, 900);
        LuoKentta();
        
        Cannon tykki = new Cannon(50, 60);
        PhysicsObject pelaaja = Ampuja(tykki);
        LuoAjastin();
        LisaaHiiri();
        Timer.SingleShot(30, LisaaEtu);

        topLista = DataStorage.TryLoad<ScoreList>(topLista, "pisteet.xml");

        
        AsetaOhjaimet(pelaaja, tykki);
        
    }

    
    /// <summary>
    /// Luodaan kenttä ja karkkikasa
    /// </summary>
    private void LuoKentta()
    {
        //Level.Background.Color = Color.White;
        Level.Background.Image = taustakuva;
        Level.Height = 1200;
        Camera.ZoomToLevel();

        PhysicsObject karkit = PhysicsObject.CreateStaticObject(Level.Width, 200, Shape.Rectangle);
        karkit.Image = karkkikasa;
        karkit.X = 0;
        karkit.Y = Level.Bottom+100;
        karkit.Color = Color.Black;
        karkit.Tag = "karkit";
        Add(karkit);
    }
    
    
    /// <summary>
    /// Lisätään tykki kentälle
    /// </summary>
    /// <returns>tykki</returns>
    private PhysicsObject Ampuja(Cannon tykki)
    {
        PhysicsObject pelaaja = PhysicsObject.CreateStaticObject(100, 100, Shape.Circle);
        pelaaja.Color = Color.Transparent;
        pelaaja.X = 0;
        pelaaja.Y = Level.Bottom+200;
        pelaaja.IgnoresCollisionResponse=true;
        pelaaja.Tag = "pelaaja";
        Add(pelaaja);

        
        tykki.FireRate = 5;
        tykki.ProjectileCollision = AmmusOsui;
        tykki.Position = pelaaja.Position;
        tykki.AttackSound = null;
        tykki.Angle = Angle.FromDegrees(90);
        tykki.Image = tykkikuva;
        tykki.RotateImage = false;
        tykki.Tag = "tykki";
        pelaaja.Add(tykki);
        return pelaaja;
        
    }

    /// <summary>
    /// Luodaan hiiret
    /// </summary>
    /// <returns></returns>
    private void  LisaaHiiri()
    {
        PhysicsObject hiiri = new PhysicsObject(60, 100, Shape.Circle);
        hiiri.Animation = new Animation(hiirikuva);
        hiiri.Animation.Start();
        hiiri.Color = Color.Black;
        hiiri.X = RandomGen.NextDouble(Level.Left+10, Level.Right-10);
        hiiri.Y = Level.Top;
        hiiri.Velocity = new Vector(0, nopeusHiiri);
        hiiri.Tag = "hiiri";
        hiiri.CollisionIgnoreGroup = 1;
        Add(hiiri);
        
        AddCollisionHandler(hiiri, "karkit", PeliLoppuu);
        Timer.SingleShot(RandomGen.NextDouble(1, 2), LisaaHiiri);
    }
    
    private void  LisaaEtu()
    {
        PhysicsObject etu = LuoEtu(RandomGen.NextDouble(Level.Left, Level.Right), Level.Top);
        //PhysicsObject etu = new PhysicsgiObject(60, 60, Shape.Circle);
        //etu.Color = Color.Black;
        //etu.X = RandomGen.NextDouble(Level.Left, Level.Right);
        //etu.Y = Level.Top;
        //etu.Velocity = new Vector(0, -100);
        //etu.Tag = "etu";
        //etu.Image = etukuva;
        //etu.CollisionIgnoreGroup = 1;
        etu.MakeStatic();
        Timer.SingleShot(9.1, delegate
        {
            double x = etu.X;
            double y = etu.Y;
            etu.Destroy();
            etu = LuoEtu(x, y);
        });
        

        

        Timer.SingleShot(30 , LisaaEtu);
    }

    private PhysicsObject LuoEtu(double x, double y)
    {
        PhysicsObject etu = new PhysicsObject(60, 60, Shape.Circle);
        etu.Color = Color.Black;
        etu.X = x;
        etu.Y = y;
        etu.Velocity = new Vector(0, -100);
        etu.Tag = "etu";
        etu.Image = etukuva;
        etu.CollisionIgnoreGroup = 1;
        Add(etu);
        AddCollisionHandler(etu, "karkit", EtuKatoaa);
        AddCollisionHandler(etu, "pelaaja", NopeusHidastuu);
        return etu;
        
        
    }

    private static void EtuKatoaa(PhysicsObject etu, PhysicsObject kohde)
    {
        etu.Destroy();
    }
    
    private void NopeusHidastuu(PhysicsObject etu, PhysicsObject kohde)
    {
        double a = nopeusHiiri / 2;
        etu.Destroy();
        Timer.SingleShot(10, delegate { nopeusHiiri = nopeusHiiri+a;});
        nopeusHiiri = a;
    }

    
    private void LuoAjastin()
    {
        ajastin = new Timer();
        ajastin.Interval = 5;
        ajastin.Timeout += delegate { nopeusHiiri = nopeusHiiri - 10; };
        ajastin.Start();
        
        Label aikanaytto = new Label();
        aikanaytto.X = Level.Left+200;
        aikanaytto.Y = Level.Top-200;
        aikanaytto.TextColor = Color.Black;
        aikanaytto.DecimalPlaces = 2;
        aikanaytto.BindTo(ajastin.SecondCounter);
        Add(aikanaytto);
        
        

    }

    
    /// <summary>
    /// Määrittää osuman tapahtuman
    /// </summary>
    /// <param name="ammus"></param>
    /// <param name="kohde"></param>
    void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {

        if (kohde.Tag == "hiiri" || kohde.Tag == "etu")
        {
            kohde.Destroy();
            ammus.Destroy();
        }
    }

    private void PeliLoppuu(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        IsPaused = true;
        double aikaaKulunut = ajastin.SecondCounter.Value;
        aikaaKulunut = double.Round(aikaaKulunut, 2);
        MultiSelectWindow loppuvalikko = new MultiSelectWindow("GAME OVER", "Aloita alusta", "Lopeta");
        Add(loppuvalikko);
       loppuvalikko.AddItemHandler(0, AloitaPeli);
       loppuvalikko.AddItemHandler(1, Exit);
       loppuvalikko.SetButtonColor(Color.FromHexCode("#FFC671"));
       loppuvalikko.SetButtonTextColor(Color.Black);
       loppuvalikko.QuestionLabel.Font = new Font(100, true);
       loppuvalikko.QuestionLabel.TextColor = Color.Black;
       loppuvalikko.Y = 200;
       
       HighScoreWindow topIkkuna = new HighScoreWindow(
           "Parhaat pisteet",
           "Onneksi olkoon, pääsit listalle pisteillä %p! Syötä nimesi:",
           topLista, aikaaKulunut);
       topIkkuna.Closed += TallennaPisteet;
       topIkkuna.Y = -200;
       topIkkuna.NameInputWindow.Y = -100;
       topIkkuna.NameInputWindow.InputBox.Color = Color.FromHexCode("#FFC671");
       topIkkuna.NameInputWindow.OKButton.Color =Color.FromHexCode("#FFC671");
       topIkkuna.NameInputWindow.OKButton.TextColor =Color.Black;
       topIkkuna.OKButton.Color = Color.FromHexCode("#FFC671");
       topIkkuna.OKButton.TextColor =Color.Black;
       topIkkuna.List.ScoreColor = Color.Orange;
       topIkkuna.List.PositionColor = Color.Black;
       
       Add(topIkkuna);
       //Timer.SingleShot(1, topIkkuna.NameInputWindow);


    }
    private void TallennaPisteet(Window sender)
    {
        DataStorage.Save<ScoreList>(topLista, "pisteet.xml");
    }
    private void AloitaPeli()
    {
            ClearAll();
            Begin();
    }
    
    
    /// <summary>
    /// Määrittää näppäimet
    /// </summary>
    /// <param name="tykki">liittä komennon tykkiin</param>
    private void AsetaOhjaimet(PhysicsObject pelaaja, Cannon tykki)
    {
        Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeus, "Liikuta tykkiä vasemmalle", pelaaja, nopeusVasen);
        Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeus, "Liikuta tykkiä oikealle", pelaaja, nopeusOikea);
        Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        
        Keyboard.Listen(Key.Space, ButtonState.Pressed, AmmuAseella, "Ammu", tykki);

        
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
    

    /// <summary>
    /// asetetaan tykin liikuttamiselle nopeus
    /// </summary>
    /// <param name="tykki">tykki</param>
    /// <param name="nopeus">nopeus</param>
    private void AsetaNopeus(PhysicsObject tykki, Vector nopeus)
    {
        if ((nopeus.X < 0) && (tykki.Left < Level.Left))
        {
            tykki.Velocity = Vector.Zero;
            return;
        }
        
        if ((nopeus.X > 0) && (tykki.Right > Level.Right))
        {
            tykki.Velocity = Vector.Zero;
            return;
        }

        tykki.Velocity = nopeus;
    }
    

    /// <summary>
    /// Määrittää aseen ammuksen
    /// </summary>
    /// <param name="ase">käytetty ase</param>
    void AmmuAseella(Cannon ase)
    {
        PhysicsObject ammus = ase.Shoot();


        if (ammus != null)
        {
            ammus.Tag = "ammus";
            ammus.Size *= 2;
            ammus.Image = null;
            ammus.Color = Color.Black;
            ammus.MakeStatic();
        }
    }
}