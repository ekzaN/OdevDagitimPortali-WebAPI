using odevDagitimPortali2.Models;
using odevDagitimPortali2.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace odevDagitimPortali2.Controllers
{
    public class ServisController : ApiController
    {
        DB01Entities db = new DB01Entities();
        SonucModel sonuc = new SonucModel();

        #region Ogrenci

        [HttpGet]
        [Route("api/ogrenciliste")]
        public List<OgrenciModel> OgrenciListe()
        {
            List<OgrenciModel> liste = db.Ogrenci.Select(x => new OgrenciModel()
            {
                ogrId = x.ogrId,
                ogrNo = x.ogrNo,
                ogrAdsoyad = x.ogrAdsoyad,
                ogrDogTarih = x.ogrDogTarih,
                ogrFoto = x.ogrFoto,
            }).ToList();
            return liste;
        }


        [HttpGet]
        [Route("api/ogrencibyid{ogrId}")]
        public OgrenciModel OgrenciById(string ogrId)
        {
            OgrenciModel kayit = db.Ogrenci.Where(s => s.ogrId == ogrId).Select(x => new OgrenciModel()
            {
                ogrId = x.ogrId,
                ogrNo = x.ogrNo,
                ogrAdsoyad = x.ogrAdsoyad,
                ogrDogTarih = x.ogrDogTarih,
                ogrFoto = x.ogrFoto,
            }).SingleOrDefault();
            return kayit;
        }


        [HttpPost]
        [Route("api/ogrenciekle")]
        public SonucModel OgrenciEkle(OgrenciModel model)
        {
            if (db.Ogrenci.Count(s => s.ogrNo == model.ogrNo) > 0)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Girilen Öğrenci Numarası Kayıtlıdır!";
            }

            Ogrenci yeni = new Ogrenci();
            yeni.ogrId = Guid.NewGuid().ToString();
            yeni.ogrNo = model.ogrNo;
            yeni.ogrAdsoyad = model.ogrAdsoyad;
            yeni.ogrDogTarih = model.ogrDogTarih;
            yeni.ogrFoto = model.ogrFoto;
            db.Ogrenci.Add(yeni);
            db.SaveChanges();
            sonuc.islem = true;
            sonuc.mesaj = "Yeni Öğrenci Eklendi";
            return sonuc;
        }
        [HttpPut]
        [Route("api/ogrenciduzenle")]
        public SonucModel OgrenciDuzenle(OgrenciModel model)
        {
            Ogrenci kayit = db.Ogrenci.Where(s => s.ogrId == model.ogrId).SingleOrDefault();
            if (kayit == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Öğrenci Bulunamadı!";
                return sonuc;
            }
            kayit.ogrNo = model.ogrNo;
            kayit.ogrAdsoyad = model.ogrAdsoyad;
            kayit.ogrDogTarih = model.ogrDogTarih;
            kayit.ogrFoto = model.ogrFoto;
            db.SaveChanges();
            sonuc.islem = true;
            sonuc.mesaj = "Öğrenci Düzenlendi!";
            return sonuc;
        }
        [HttpDelete]
        [Route("api/ogrencisil/{ogrId}")]
        public SonucModel OgrenciSil(string ogrId)
        {
            Ogrenci kayit = db.Ogrenci.Where(s => s.ogrId == ogrId).SingleOrDefault();

            if (kayit == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Öğrenci Bulunamadı!";
                return sonuc;
            }
            if (db.Kayit.Count(s => s.kayitOgrId == ogrId) > 0)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Öğrenci Üzerinde Ders Kaydı Olduğu İçin Öğrenci Silinemiyor!";
                return sonuc;
            }
            db.Ogrenci.Remove(kayit);
            db.SaveChanges();
            sonuc.islem = true;
            sonuc.mesaj = "Öğrenci  Silindi";
            return sonuc;
        }

        [HttpPost]
        [Route("api/ogrfotoguncelle")]
        public SonucModel OgrFotoGuncelle(ogrFotoModel model)
        {
            Ogrenci ogr = db.Ogrenci.Where(s => s.ogrFoto == model.ogrId).SingleOrDefault();

            if (ogr == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Kayıt Bulunamadı!";

                return sonuc;
            }

            if (ogr.ogrFoto != "profil.jpg")
            {
                string yol = System.Web.Hosting.HostingEnvironment.MapPath("~/Folders/" + ogr.ogrFoto);

                if (File.Exists(yol))
                {
                    File.Delete(yol);
                }
            }

            string data = model.fotoData;
            string base64 = data.Substring(data.IndexOf(',') + 1); base64 = base64.Trim('\0');
            byte[] imgbytes = Convert.FromBase64String(base64);
            string dosyaAdi = ogr.ogrId + model.dosyaUzanti.Replace("image/", ".");

            using (var ms = new MemoryStream(imgbytes, 0, imgbytes.Length))
            {
                Image img = Image.FromStream(ms, true);
                img.Save(System.Web.Hosting.HostingEnvironment.MapPath("~/Folders/" + dosyaAdi));
            }
            ogr.ogrFoto = dosyaAdi; db.SaveChanges();

            sonuc.islem = true;
            sonuc.mesaj = "Fotoğraf Güncellendi";

            return sonuc;

        }
        #endregion

        #region Kayit
        [HttpGet]
        [Route("api/ogrencidersliste/{ogrId}")]
        public List<KayitModel> OgrenciDersListe(string ogrId)
        {
            List<KayitModel> liste = db.Kayit.Where(s => s.kayitOgrId == ogrId).Select(x => new KayitModel()
            {
                kayitId = x.kayitId,
                kayitDersId = x.kayitDersId,
                kayitOgrId = x.kayitOgrId,
            }).ToList();

            foreach (var kayit in liste)

            {
                kayit.ogrBilgi = OgrenciById(kayit.kayitOgrId);
                kayit.dersBilgi = DersById(kayit.kayitDersId);
            }
            return liste;
        }
        [HttpGet]
        [Route("api/dersogrenciliste/{dersId}")]
        public List<KayitModel> DersOgrenciListe(string dersId)
        {
            List<KayitModel> liste = db.Kayit.Where(s => s.kayitDersId == dersId).Select(x => new KayitModel()
            {
                kayitId = x.kayitId,
                kayitDersId = x.kayitDersId,
                kayitOgrId = x.kayitOgrId,
            }).ToList();
            foreach (var kayit in liste)
            {
                kayit.ogrBilgi = OgrenciById(kayit.kayitOgrId);
                kayit.dersBilgi = DersById(kayit.kayitDersId);
            }
            return liste;
        }
        [HttpPost]
        [Route("api/kayitekle")]
        public SonucModel KayitEkle(KayitModel model)
        {
            if (db.Kayit.Count(s => s.kayitDersId == model.kayitDersId && s.kayitOgrId == model.kayitOgrId) > 0)
            {
                sonuc.islem = false;
                sonuc.mesaj = "İlgili Öğrenci Derse Önceden Kayıtlıdır!";
                return sonuc;
            }
            Kayit yeni = new Kayit();
            yeni.kayitId = Guid.NewGuid().ToString();
            yeni.kayitOgrId = model.kayitOgrId;
            yeni.kayitDersId = model.kayitDersId;
            db.Kayit.Add(yeni);
            db.SaveChanges();
            sonuc.islem = true;
            sonuc.mesaj = "Ders Kaydı Eklendi";
            return sonuc;
        }
        [HttpDelete]
        [Route("api/kayitsil/{kayitId}")]
        public SonucModel KayitSil(string kayitId)
        {
            Kayit kayit = db.Kayit.Where(s => s.kayitId == kayitId).SingleOrDefault();
            if (kayit == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Kayıt Bulunamadı!";
                return sonuc;
            }
            db.Kayit.Remove(kayit);
            db.SaveChanges();
            sonuc.islem = true;
            sonuc.mesaj = "Ders Kaydı Silindi";
            return sonuc;
        }
        #endregion

        #region Ders


        [HttpGet]
        [Route("api/dersliste")]

        public List<DersModel> DersListe()
        {
            List<DersModel> liste = db.Ders.Select(x => new DersModel()
            {
                dersId = x.dersId,
                dersKodu = x.dersKodu,
                dersAdi = x.dersAdi,
                dersKredi = x.dersKredi
            }).ToList();

            return liste;
        }


        [HttpGet]
        [Route("api/dersbyid/{dersId}")]
        public DersModel DersById(string dersId)
        {
            DersModel kayit = db.Ders.Where(s => s.dersId == dersId).Select(x => new DersModel()
            {
                dersId = x.dersId,
                dersKodu = x.dersKodu,
                dersAdi = x.dersAdi,
                dersKredi = x.dersKredi

            }).SingleOrDefault();
            return kayit;
        }


        [HttpPost]
        [Route("api/dersekle")]
        public SonucModel DersEkle(DersModel model)
        {
            if (db.Ders.Count(s => s.dersKodu == model.dersKodu) > 0)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Girilen Ders Kodu Kayıtlıdır !";
                return sonuc;
            }

            Ders yeni = new Ders();
            yeni.dersId = Guid.NewGuid().ToString();
            yeni.dersKodu = model.dersKodu;
            yeni.dersAdi = model.dersAdi;
            yeni.dersKredi = model.dersKredi;
            db.Ders.Add(yeni);
            db.SaveChanges();
            sonuc.islem = true;
            sonuc.mesaj = "Ders Eklendi";


            return sonuc;
        }

        [HttpPut]
        [Route("api/dersduzenle")]

        public SonucModel DersDuzenle(DersModel model)
        {
            Ders kayit = db.Ders.Where(s => s.dersId == model.dersId).SingleOrDefault();

            if (kayit == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Ders Bulunamadı!";
                return sonuc;
            }

            kayit.dersKodu = model.dersKodu;
            kayit.dersAdi = model.dersAdi;
            kayit.dersKredi = model.dersKredi;
            db.SaveChanges();

            sonuc.islem = true;
            sonuc.mesaj = "Ders Adı Düzenlendi!";

            return sonuc;
        }


        [HttpDelete]
        [Route("api/derssil/{dersId}")]
        public SonucModel DersSil(string dersId)
        {
            Ders kayit = db.Ders.Where(s => s.dersId == s.dersId).SingleOrDefault();

            if (kayit == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Ders Bulunamadı!";
                return sonuc;
            }
            db.Ders.Remove(kayit);
            db.SaveChanges();
            sonuc.islem = true;
            sonuc.mesaj = "Ders Silindi!";


            return sonuc;
        }
        #endregion

        #region Odev

        [HttpGet]
        [Route("api/odevliste")]

        public List<OdevModel> OdevListe()
        {
            List<OdevModel> liste = db.Odev.Select(x => new OdevModel()
            {
                odevId = x.odevId,
                odevAdi = x.odevAdi,
                odevDersId = x.odevDersId
            }).ToList();

            return liste;
        }


        [HttpGet]
        [Route("api/odevbyid/{odevId}")]
        public OdevModel OdevById(string odevId)
        {
            OdevModel kayit = db.Odev.Where(s => s.odevId == odevId).Select(x => new OdevModel()
            {
                odevId = x.odevId,
                odevAdi = x.odevAdi,
                odevDersId = x.odevDersId

            }).SingleOrDefault();

            return kayit;
        }

        [HttpPost]
        [Route("api/odevekle")]
        public SonucModel OdevEkle(OdevModel model)
        {
            if (db.Odev.Count(s => s.odevId == model.odevId) > 0)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Girilen Ödev Kayıtlıdır !";
                return sonuc;
            }

            Odev yeni = new Odev();
            yeni.odevId = Guid.NewGuid().ToString();
            yeni.odevAdi = model.odevAdi;
            yeni.odevDersId = model.odevDersId;

            db.Odev.Add(yeni);
            db.SaveChanges();

            sonuc.islem = true;
            sonuc.mesaj = "Ödev Eklendi.";


            return sonuc;
        }

        [HttpPut]
        [Route("api/odevduzenle")]

        public SonucModel OdevDuzenle(OdevModel model)
        {
            Odev kayit = db.Odev.Where(s => s.odevAdi == model.odevAdi).SingleOrDefault();

            if (kayit == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Ödev Bulunamadı!";
                return sonuc;
            }

            kayit.odevId = Guid.NewGuid().ToString();
            kayit.odevAdi = model.odevAdi;
            kayit.odevDersId = model.odevDersId;
            db.SaveChanges();

            sonuc.islem = true;
            sonuc.mesaj = "Ödev Adı Düzenlendi!";

            return sonuc;
        }


        [HttpDelete]
        [Route("api/odevsil/{odevId}")]
        public SonucModel OdevSil(string odevId)
        {
            Odev kayit = db.Odev.Where(s => s.odevId == s.odevId).SingleOrDefault();

            if (kayit == null)
            {
                sonuc.islem = false;
                sonuc.mesaj = "Ödev Bulunamadı!";
                return sonuc;
            }
            db.Odev.Remove(kayit);
            db.SaveChanges();

            sonuc.islem = true;
            sonuc.mesaj = "Ödev Silindi!";


            return sonuc;
        }

        #endregion

    }
}
