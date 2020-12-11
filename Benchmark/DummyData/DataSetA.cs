using System;
using System.Linq;

namespace Cistern.Benchmarks.DummyData
{
    public static class DataSetA
    {
        public class Data
        {
			public string Name { get; set; }
			public string Street { get; set; }
			public string City { get; set; }
			public string PostCode { get; set; }
			public string Country { get; set; }
			public string Phone { get; set; }
			public DateTime DOB { get; set; }
		}

		public static Data[] Get() =>
			new [] {
				new { Name = "Koch, Mira R.", Street = "613-5153 Quam Rd.", City = "Nottingham", PostCode = "069979", Country = "Martinique", Phone = "(05) 8535 8225", DOB = "2021-02-28" },
				new { Name = "Berger, Gisela W.", Street = "304 Metus Ave", City = "Buner", PostCode = "3224", Country = "Ethiopia", Phone = "(08) 8369 0629", DOB = "2021-02-07" },
				new { Name = "George, Sonia B.", Street = "P.O. Box 539, 5599 Vestibulum, Road", City = "Lang", PostCode = "943636", Country = "Sint Maarten", Phone = "(02) 0312 8415", DOB = "2021-09-29" },
				new { Name = "Barlow, Cyrus S.", Street = "547 Vehicula. Avenue", City = "Haßloch", PostCode = "11908", Country = "Malawi", Phone = "(02) 1371 5979", DOB = "2021-06-23" },
				new { Name = "Butler, Hope M.", Street = "Ap #979-5437 Sapien. Road", City = "Catemu", PostCode = "504292", Country = "Malta", Phone = "(05) 0885 2483", DOB = "2021-04-18" },
				new { Name = "Mendoza, Mira K.", Street = "339-4764 Phasellus Road", City = "Milena", PostCode = "0953", Country = "Serbia", Phone = "(02) 4853 4823", DOB = "2021-06-20" },
				new { Name = "Frazier, Xyla X.", Street = "1739 Luctus Street", City = "Colwood", PostCode = "P3Y 0PT", Country = "Belize", Phone = "(05) 7273 9165", DOB = "2020-12-28" },
				new { Name = "Dunlap, Hiram J.", Street = "P.O. Box 849, 524 Eget, St.", City = "Gulfport", PostCode = "450454", Country = "Tunisia", Phone = "(05) 6290 0007", DOB = "2021-01-14" },
				new { Name = "Guerrero, Daryl C.", Street = "P.O. Box 557, 5644 Molestie Avenue", City = "Serrata", PostCode = "62-630", Country = "Uganda", Phone = "(06) 2162 0636", DOB = "2021-05-23" },
				new { Name = "Frost, Ian J.", Street = "Ap #608-3975 Lacinia Av.", City = "Petit-Hallet", PostCode = "56751", Country = "Albania", Phone = "(01) 4474 2988", DOB = "2021-02-01" },
				new { Name = "Hanson, Chaim I.", Street = "Ap #537-9035 Faucibus St.", City = "St. John's", PostCode = "01279", Country = "Korea, South", Phone = "(07) 5983 4844", DOB = "2021-11-14" },
				new { Name = "Fowler, Jane A.", Street = "P.O. Box 205, 4911 Fusce St.", City = "Allahabad", PostCode = "18124", Country = "Virgin Islands, British", Phone = "(07) 1152 4107", DOB = "2020-10-17" },
				new { Name = "Dejesus, Samuel Y.", Street = "Ap #437-9213 Eget Road", City = "San Felipe", PostCode = "67417", Country = "Laos", Phone = "(09) 8659 2665", DOB = "2019-12-29" },
				new { Name = "Wood, Unity Y.", Street = "P.O. Box 304, 9993 Enim St.", City = "João Pessoa", PostCode = "35244", Country = "Bermuda", Phone = "(03) 5466 5828", DOB = "2021-02-08" },
				new { Name = "Harrell, Ima X.", Street = "7276 Erat Street", City = "Scunthorpe", PostCode = "53229", Country = "Jordan", Phone = "(08) 3341 8620", DOB = "2020-10-25" },
				new { Name = "Ryan, Axel J.", Street = "P.O. Box 342, 2848 Auctor Road", City = "Isca sullo Ionio", PostCode = "07872-222", Country = "Hungary", Phone = "(08) 1232 9839", DOB = "2021-03-13" },
				new { Name = "Baxter, Brady G.", Street = "4077 Cursus Avenue", City = "Santa Maria a Monte", PostCode = "9108 FQ", Country = "Comoros", Phone = "(02) 5327 6125", DOB = "2021-11-11" },
				new { Name = "Frazier, Colleen H.", Street = "102 Erat St.", City = "Williams Lake", PostCode = "3795", Country = "Nicaragua", Phone = "(01) 4468 0039", DOB = "2021-06-29" },
				new { Name = "Owen, Tatiana M.", Street = "3827 Erat St.", City = "Cumberland", PostCode = "L6J 2X6", Country = "Christmas Island", Phone = "(09) 7506 0412", DOB = "2021-05-30" },
				new { Name = "Moore, Lyle Q.", Street = "764-2217 Justo Av.", City = "Tongyeong", PostCode = "6159", Country = "Hong Kong", Phone = "(09) 9792 3844", DOB = "2021-08-11" },
				new { Name = "Henson, Harding D.", Street = "338-6830 At Avenue", City = "Gwangju", PostCode = "48211", Country = "Mayotte", Phone = "(03) 4675 7754", DOB = "2021-05-26" },
				new { Name = "Lloyd, Gillian P.", Street = "3064 Lorem Street", City = "Washuk", PostCode = "66880", Country = "Nicaragua", Phone = "(06) 9573 7158", DOB = "2020-05-04" },
				new { Name = "Alford, Kasper G.", Street = "Ap #196-5258 Euismod Street", City = "Dignano", PostCode = "24893", Country = "Benin", Phone = "(07) 2890 6451", DOB = "2020-08-11" },
				new { Name = "Jefferson, Shoshana K.", Street = "Ap #977-239 Enim Street", City = "Stavoren", PostCode = "R4Z 2N3", Country = "Guernsey", Phone = "(05) 7100 8621", DOB = "2021-05-04" },
				new { Name = "Hoover, Mercedes E.", Street = "P.O. Box 216, 3610 Turpis. Road", City = "Arbre", PostCode = "67424", Country = "Congo, the Democratic Republic of the", Phone = "(02) 3617 1265", DOB = "2020-06-04" },
				new { Name = "Ballard, Bryar F.", Street = "Ap #556-3437 Sapien. Rd.", City = "Belgorod", PostCode = "31100", Country = "Yemen", Phone = "(03) 0693 1067", DOB = "2021-01-23" },
				new { Name = "Fuentes, Chadwick S.", Street = "9725 A Road", City = "Villahermosa", PostCode = "7271", Country = "Lebanon", Phone = "(08) 5627 1929", DOB = "2021-10-11" },
				new { Name = "Branch, Claudia C.", Street = "911-6397 Quisque Road", City = "Nuneaton", PostCode = "6300", Country = "Kuwait", Phone = "(02) 9700 4677", DOB = "2021-06-11" },
				new { Name = "Poole, Mannix O.", Street = "Ap #463-777 At, Rd.", City = "Gellik", PostCode = "30506", Country = "Guernsey", Phone = "(03) 0200 7767", DOB = "2020-08-01" },
				new { Name = "Sharpe, Miriam T.", Street = "728-1388 Duis Street", City = "Strona", PostCode = "260244", Country = "Armenia", Phone = "(02) 2335 1442", DOB = "2020-11-06" },
				new { Name = "Beach, Stephanie Y.", Street = "Ap #927-5403 Arcu Avenue", City = "Cádiz", PostCode = "71305", Country = "Suriname", Phone = "(06) 2688 7550", DOB = "2021-03-13" },
				new { Name = "Clayton, Amery J.", Street = "2333 Vitae Avenue", City = "Sainte-Flavie", PostCode = "UY45 1ON", Country = "Peru", Phone = "(05) 0322 8918", DOB = "2021-06-01" },
				new { Name = "Browning, Griffith A.", Street = "9107 Faucibus Av.", City = "Maaseik", PostCode = "11818", Country = "Serbia", Phone = "(04) 1114 8994", DOB = "2020-01-11" },
				new { Name = "Austin, Hiram Z.", Street = "P.O. Box 270, 2814 Arcu. Street", City = "Bornival", PostCode = "99-465", Country = "Trinidad and Tobago", Phone = "(06) 3104 2246", DOB = "2021-12-05" },
				new { Name = "Caldwell, Gillian T.", Street = "144-3189 Nulla Avenue", City = "Minna", PostCode = "3812", Country = "Latvia", Phone = "(05) 8404 4226", DOB = "2020-08-14" },
				new { Name = "Boyd, Odessa M.", Street = "Ap #944-5625 Magna Ave", City = "Itanagar", PostCode = "Z9620", Country = "Saint Vincent and The Grenadines", Phone = "(05) 3459 0188", DOB = "2020-04-07" },
				new { Name = "Bass, Jordan U.", Street = "832-4804 Libero Road", City = "Muzaffargarh", PostCode = "458614", Country = "Israel", Phone = "(09) 1261 5549", DOB = "2021-12-01" },
				new { Name = "Soto, Malik O.", Street = "Ap #119-134 Nunc Road", City = "Perugia", PostCode = "15112", Country = "South Sudan", Phone = "(05) 0677 1038", DOB = "2021-09-25" },
				new { Name = "Kinney, Oleg W.", Street = "4095 In Ave", City = "Hualpén", PostCode = "604337", Country = "Slovakia", Phone = "(02) 7383 1696", DOB = "2020-12-12" },
				new { Name = "Jensen, Wade T.", Street = "P.O. Box 164, 7827 Duis Avenue", City = "Baidyabati", PostCode = "62842", Country = "Zimbabwe", Phone = "(02) 1975 8872", DOB = "2020-07-20" },
				new { Name = "Armstrong, Declan O.", Street = "Ap #611-9843 Ornare Av.", City = "Coatzacoalcos", PostCode = "89953", Country = "France", Phone = "(02) 9387 1024", DOB = "2021-03-21" },
				new { Name = "Webster, Demetria Q.", Street = "Ap #302-1324 Et St.", City = "Okene", PostCode = "661443", Country = "Monaco", Phone = "(03) 6799 9115", DOB = "2021-01-14" },
				new { Name = "Rogers, Quin U.", Street = "5197 Dui, St.", City = "Coldstream", PostCode = "0729 VT", Country = "Virgin Islands, United States", Phone = "(02) 1506 4188", DOB = "2021-04-18" },
				new { Name = "Norton, Shoshana Y.", Street = "Ap #643-3168 Nunc Rd.", City = "Burg", PostCode = "412499", Country = "Botswana", Phone = "(07) 2866 1946", DOB = "2020-11-07" },
				new { Name = "Conway, Yoko Z.", Street = "Ap #250-8362 Non, Road", City = "Kitchener", PostCode = "17112", Country = "Uruguay", Phone = "(02) 3499 7626", DOB = "2020-01-01" },
				new { Name = "Mann, Jayme T.", Street = "P.O. Box 510, 1485 Nullam St.", City = "Maaseik", PostCode = "8517", Country = "Chile", Phone = "(06) 1040 2016", DOB = "2020-10-04" },
				new { Name = "Glass, Caesar Q.", Street = "P.O. Box 901, 6883 Sit Avenue", City = "Carbonear", PostCode = "66991", Country = "Ethiopia", Phone = "(07) 8607 2994", DOB = "2021-01-16" },
				new { Name = "Sexton, Walter J.", Street = "6361 Libero. Rd.", City = "Bolano", PostCode = "44410-13927", Country = "Cameroon", Phone = "(07) 1178 1982", DOB = "2021-09-22" },
				new { Name = "Blankenship, Yardley Y.", Street = "P.O. Box 706, 9875 Dolor Ave", City = "Champion", PostCode = "896021", Country = "United Arab Emirates", Phone = "(03) 5418 4692", DOB = "2021-01-05" },
				new { Name = "Hoffman, Quemby P.", Street = "313-8916 Id Av.", City = "Torgny", PostCode = "HM53 9RA", Country = "Ukraine", Phone = "(07) 2297 8646", DOB = "2020-08-06" },
				new { Name = "Copeland, Karen C.", Street = "5146 Non St.", City = "Rignano Garganico", PostCode = "82574", Country = "Antigua and Barbuda", Phone = "(02) 5381 7141", DOB = "2021-10-27" },
				new { Name = "Mendez, Lacota J.", Street = "1594 Eu, St.", City = "Zaventem", PostCode = "03892", Country = "Denmark", Phone = "(06) 0235 3406", DOB = "2020-09-21" },
				new { Name = "Bell, Sean J.", Street = "4752 Ad Street", City = "Panketal", PostCode = "063892", Country = "New Caledonia", Phone = "(04) 2514 6127", DOB = "2021-10-21" },
				new { Name = "Montoya, Fitzgerald X.", Street = "Ap #125-9957 Magna. St.", City = "Borgo Valsugana", PostCode = "8516", Country = "Oman", Phone = "(03) 8373 3659", DOB = "2020-02-04" },
				new { Name = "Wolfe, Plato H.", Street = "6404 Eget St.", City = "Los Angeles", PostCode = "Z7726", Country = "Estonia", Phone = "(03) 1125 6067", DOB = "2020-09-11" },
				new { Name = "Hodge, Cooper A.", Street = "888-953 Congue. St.", City = "Cupar", PostCode = "785407", Country = "Uganda", Phone = "(02) 5749 8530", DOB = "2020-01-27" },
				new { Name = "Wiggins, Madonna Z.", Street = "P.O. Box 621, 139 Lorem, Rd.", City = "Maple Ridge", PostCode = "02105", Country = "Samoa", Phone = "(06) 7236 2483", DOB = "2020-03-02" },
				new { Name = "Ray, Mannix L.", Street = "P.O. Box 775, 6851 Magna. Av.", City = "Bucheon", PostCode = "6818 KD", Country = "Puerto Rico", Phone = "(09) 3652 3788", DOB = "2020-01-15" },
				new { Name = "Valentine, Alec H.", Street = "P.O. Box 611, 3446 Eu Street", City = "Berloz", PostCode = "G2K 6H5", Country = "Tonga", Phone = "(05) 8720 8475", DOB = "2020-04-27" },
				new { Name = "Greene, Hannah E.", Street = "P.O. Box 937, 1250 Ipsum Road", City = "Maglie", PostCode = "6273", Country = "Burkina Faso", Phone = "(06) 4719 1485", DOB = "2020-04-16" },
				new { Name = "Boyer, Silas H.", Street = "891-1856 Est. Street", City = "Hualpén", PostCode = "20-999", Country = "Aruba", Phone = "(04) 1764 8735", DOB = "2021-03-19" },
				new { Name = "Mooney, Ginger I.", Street = "Ap #625-9489 Tincidunt St.", City = "Auburn", PostCode = "63001", Country = "Saint Vincent and The Grenadines", Phone = "(08) 5021 5870", DOB = "2021-08-13" },
				new { Name = "Jarvis, Bruno J.", Street = "859-9130 Commodo St.", City = "Aiseau-Presles", PostCode = "05554", Country = "Australia", Phone = "(09) 3221 3874", DOB = "2021-07-16" },
				new { Name = "Weiss, September L.", Street = "Ap #981-7222 Fermentum Ave", City = "Borgo Valsugana", PostCode = "33627-726", Country = "Kyrgyzstan", Phone = "(01) 6879 5277", DOB = "2020-04-07" },
				new { Name = "Mercado, Shoshana U.", Street = "P.O. Box 820, 6251 Risus. Road", City = "Mobile", PostCode = "318903", Country = "French Guiana", Phone = "(09) 3245 5738", DOB = "2020-02-16" },
				new { Name = "Wilkerson, Dalton F.", Street = "P.O. Box 253, 1613 Nunc Rd.", City = "Grand-Reng", PostCode = "66834", Country = "Falkland Islands", Phone = "(05) 4657 4687", DOB = "2020-11-23" },
				new { Name = "Medina, Jena B.", Street = "P.O. Box 733, 4858 Neque St.", City = "Barchi", PostCode = "10480", Country = "Norway", Phone = "(03) 2525 7648", DOB = "2020-06-11" },
				new { Name = "Robertson, Kato B.", Street = "P.O. Box 460, 4483 Feugiat St.", City = "Sigillo", PostCode = "416321", Country = "Faroe Islands", Phone = "(04) 6763 6337", DOB = "2021-04-03" },
				new { Name = "Nichols, Colton K.", Street = "P.O. Box 455, 3506 Integer Avenue", City = "Hampstead", PostCode = "9845", Country = "United Kingdom (Great Britain)", Phone = "(04) 6750 1811", DOB = "2021-04-16" },
				new { Name = "Riley, Yvonne S.", Street = "211-4197 Suspendisse Ave", City = "Lanklaar", PostCode = "334453", Country = "Oman", Phone = "(07) 4363 8926", DOB = "2020-05-05" },
				new { Name = "Bauer, Eden P.", Street = "P.O. Box 385, 5185 Tincidunt Avenue", City = "Tebing Tinggi", PostCode = "91878-14590", Country = "Timor-Leste", Phone = "(09) 8354 2577", DOB = "2019-12-22" },
				new { Name = "Rutledge, Cora P.", Street = "P.O. Box 291, 2001 Aliquet Av.", City = "Saint-Brieuc", PostCode = "542990", Country = "Swaziland", Phone = "(02) 3041 7717", DOB = "2019-12-31" },
				new { Name = "Lawson, Maia I.", Street = "Ap #787-828 Nec, Avenue", City = "Houston", PostCode = "51-037", Country = "Saint Helena, Ascension and Tristan da Cunha", Phone = "(09) 4035 0705", DOB = "2021-03-30" },
				new { Name = "Cooley, Timon K.", Street = "Ap #972-2840 Curabitur St.", City = "Barahanagar", PostCode = "60156", Country = "Israel", Phone = "(04) 2047 9802", DOB = "2021-09-09" },
				new { Name = "Whitfield, Carissa N.", Street = "Ap #768-7592 Viverra. St.", City = "San Pedro de la Paz", PostCode = "28643", Country = "Bonaire, Sint Eustatius and Saba", Phone = "(09) 0746 8291", DOB = "2020-06-21" },
				new { Name = "Hoffman, Emerson Z.", Street = "P.O. Box 807, 6406 Pellentesque. Street", City = "Aubervilliers", PostCode = "5343", Country = "United Arab Emirates", Phone = "(03) 8676 9469", DOB = "2020-11-28" },
				new { Name = "Stewart, Jermaine O.", Street = "203-4193 Ullamcorper, Av.", City = "Saguenay", PostCode = "38158", Country = "Bahrain", Phone = "(04) 7362 5658", DOB = "2020-06-14" },
				new { Name = "Hunter, Macaulay K.", Street = "Ap #606-2964 Sociosqu Avenue", City = "Torchiarolo", PostCode = "21182", Country = "Isle of Man", Phone = "(05) 3529 0633", DOB = "2020-06-09" },
				new { Name = "Carlson, Alexander K.", Street = "P.O. Box 250, 5747 Non Avenue", City = "Heinsch", PostCode = "8685", Country = "Cocos (Keeling) Islands", Phone = "(07) 9783 7248", DOB = "2021-06-01" },
				new { Name = "Bradford, Hilda A.", Street = "7669 Sed, Rd.", City = "Frasnes-lez-Gosselies", PostCode = "8138", Country = "Bosnia and Herzegovina", Phone = "(05) 2732 5032", DOB = "2021-04-05" },
				new { Name = "Robles, Devin Z.", Street = "239 Varius Rd.", City = "Hoorn", PostCode = "6945 PL", Country = "Taiwan", Phone = "(08) 4662 9060", DOB = "2021-07-26" },
				new { Name = "Vaughn, Hiram H.", Street = "P.O. Box 843, 2403 Sed St.", City = "Philadelphia", PostCode = "M8N 9TG", Country = "Singapore", Phone = "(03) 7168 4001", DOB = "2021-09-22" },
				new { Name = "Peck, Berk U.", Street = "8823 Fringilla Rd.", City = "Söke", PostCode = "26676-012", Country = "Nepal", Phone = "(04) 7658 2809", DOB = "2021-09-12" },
				new { Name = "Pennington, Warren B.", Street = "Ap #663-2156 Varius Street", City = "Ercilla", PostCode = "61776", Country = "Algeria", Phone = "(06) 6032 3205", DOB = "2021-03-10" },
				new { Name = "Zimmerman, Rashad W.", Street = "P.O. Box 713, 7047 Sem Road", City = "Karabash", PostCode = "305657", Country = "Martinique", Phone = "(08) 9625 1289", DOB = "2020-04-04" },
				new { Name = "Dunn, Keefe R.", Street = "386-8994 Interdum. Rd.", City = "Lavoir", PostCode = "39520", Country = "Ukraine", Phone = "(09) 6041 7699", DOB = "2021-06-29" },
				new { Name = "Richard, Rajah P.", Street = "512-5872 Velit. Street", City = "Rocca Santo Stefano", PostCode = "Z9410", Country = "Solomon Islands", Phone = "(02) 9489 6140", DOB = "2020-06-02" },
				new { Name = "Dixon, Kenyon B.", Street = "3824 Elit. Road", City = "Pangnirtung", PostCode = "3062", Country = "Belgium", Phone = "(09) 1721 8523", DOB = "2020-11-25" },
				new { Name = "Shaffer, Cody O.", Street = "667-3638 Sed St.", City = "Frankfurt am Main", PostCode = "9921 XR", Country = "Austria", Phone = "(08) 7724 8374", DOB = "2021-07-29" },
				new { Name = "Fox, Clark F.", Street = "Ap #410-2185 Accumsan Rd.", City = "Werbomont", PostCode = "913332", Country = "Togo", Phone = "(09) 9373 9007", DOB = "2021-07-31" },
				new { Name = "Phelps, Valentine S.", Street = "Ap #502-6239 Sem Av.", City = "San Isidro de El General", PostCode = "21-728", Country = "Angola", Phone = "(04) 9924 7295", DOB = "2021-01-25" },
				new { Name = "Downs, Nadine Y.", Street = "P.O. Box 305, 6041 Facilisis. Rd.", City = "Novgorod", PostCode = "2239", Country = "Guinea-Bissau", Phone = "(09) 3898 3801", DOB = "2019-12-16" },
				new { Name = "Guerrero, Oscar R.", Street = "428-8462 Integer Ave", City = "Northumberland", PostCode = "131974", Country = "Ethiopia", Phone = "(04) 1458 4734", DOB = "2020-02-22" },
				new { Name = "Horton, Maile R.", Street = "Ap #350-4990 Orci St.", City = "Karak", PostCode = "25668", Country = "Spain", Phone = "(07) 8322 5923", DOB = "2020-04-01" },
				new { Name = "Mccormick, Carl A.", Street = "Ap #286-9846 In Street", City = "Moio Alcantara", PostCode = "5031", Country = "Congo, the Democratic Republic of the", Phone = "(02) 8919 9240", DOB = "2021-08-25" },
				new { Name = "Carney, Sawyer L.", Street = "103-758 Elit St.", City = "Swan Hills", PostCode = "Z8638", Country = "Saint Vincent and The Grenadines", Phone = "(06) 7141 9586", DOB = "2020-02-29" },
				new { Name = "Bates, Tate N.", Street = "P.O. Box 131, 5019 Ante. Road", City = "Minitonas", PostCode = "11502", Country = "Marshall Islands", Phone = "(02) 7044 1858", DOB = "2020-07-04" },
				new { Name = "Hart, Pascale B.", Street = "Ap #710-6729 Lectus St.", City = "Mostazal", PostCode = "52595-759", Country = "Canada", Phone = "(02) 5898 8224", DOB = "2020-03-30" },
				new { Name = "Blevins, Tatum E.", Street = "P.O. Box 130, 4725 At Street", City = "Helchteren", PostCode = "57-360", Country = "Macao", Phone = "(07) 5916 8178", DOB = "2020-10-26" },
				new { Name = "Blair, Harriet Y.", Street = "638-7865 Neque. Ave", City = "Anchorage", PostCode = "71814", Country = "Kiribati", Phone = "(07) 0038 1852", DOB = "2021-02-06" }
			}
			.Select(x => new Data { Name = x.Name, Street = x.Street, City = x.City, PostCode = x.PostCode, Country = x.Country, Phone = x.Phone, DOB = DateTime.Parse(x.DOB) })
			.ToArray();

	}
}
