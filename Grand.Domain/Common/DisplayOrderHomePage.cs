using Grand.Domain.Configuration;

namespace Grand.Domain.Common
{
    public class DisplayOrderHomePage : ISettings
    {
        //Home page categories
        public int HomePageCategories { get; set; }
        public int Pc_Xl { get; set; } = 4; //default 4
        public int Pc_Lg { get; set; } = 4; //default 4
        public int Pc_Md { get; set; } = 6; //default 6
        public int Pc_Sm { get; set; } = 6; //default 6
        public int Pc_Col { get; set; } = 12; //default 12

        //Personalized products
        public int PersonalizedProducts { get; set; }
        public int Pp_Xl { get; set; } = 3; //default 3
        public int Pp_Lg { get; set; } = 4; //default 4
        public int Pp_Md { get; set; } = 4; //default 4
        public int Pp_Sm { get; set; } = 6; //default 6
        public int Pp_Col { get; set; } = 6; //default 6

        //Recommended products
        public int RecommendedProducts { get; set; } 
        public int Rp_Xl { get; set; } = 3; //default 3
        public int Rp_Lg { get; set; } = 4; //default 4
        public int Rp_Md { get; set; } = 4; //default 4
        public int Rp_Sm { get; set; } = 6; //default 6
        public int Rp_Col { get; set; } = 6; //default 6

        //Suggested products
        public int SuggestedProducts { get; set; } 
        public int Sp_Xl { get; set; } = 3; //default 3
        public int Sp_Lg { get; set; } = 4; //default 4
        public int Sp_Md { get; set; } = 4; //default 4
        public int Sp_Sm { get; set; } = 6; //default 6
        public int Sp_Col { get; set; } = 6; //default 6

        //Home page products
        public int HomePageProducts { get; set; } 
        public int Hp_Xl { get; set; } = 3; //default 3
        public int Hp_Lg { get; set; } = 4; //default 4
        public int Hp_Md { get; set; } = 4; //default 4
        public int Hp_Sm { get; set; } = 6; //default 6
        public int Hp_Col { get; set; } = 6; //default 6

        //Home page new products
        public int HomePageNewProducts { get; set; }
        public int Np_Xl { get; set; } = 3; //default 3
        public int Np_Lg { get; set; } = 4; //default 4
        public int Np_Md { get; set; } = 4; //default 4
        public int Np_Sm { get; set; } = 6; //default 6
        public int Np_Col { get; set; } = 6; //default 6

        //Category featured products
        public int CategoryFeaturedProducts { get; set; }
        public int CF_c_Xl { get; set; } = 4; //default 4
        public int CF_c_Lg { get; set; } = 4; //default 4
        public int CF_c_Md { get; set; } = 6; //default 6
        public int CF_c_Sm { get; set; } = 6; //default 6
        public int CF_c_Col { get; set; } = 6; //default 6
        public int CF_p_Xl { get; set; } = 8; //default 8
        public int CF_p_Lg { get; set; } = 8; //default 8
        public int CF_p_Md { get; set; } = 12; //default 12
        public int CF_p_Sm { get; set; } = 12; //default 12
        public int CF_p_Col { get; set; } = 12; //default 12
        public int CF_pp_Xl { get; set; } = 6; //default 6
        public int CF_pp_Lg { get; set; } = 6; //default 6
        public int CF_pp_Md { get; set; } = 6; //default 12
        public int CF_pp_Sm { get; set; } = 6; //default 12
        public int CF_pp_Col { get; set; } = 12; //default 12

        //Home page best sellers
        public int HomePageBestSellers { get; set; }
        public int Bs_Xl { get; set; } = 3; //default 3
        public int Bs_Lg { get; set; } = 4; //default 4
        public int Bs_Md { get; set; } = 4; //default 4
        public int Bs_Sm { get; set; } = 6; //default 6
        public int Bs_Col { get; set; } = 6; //default 6

        //Home page manufacturers
        public int HomePageManufacturers { get; set; }
        public int Pm_Xl { get; set; } = 4; //default 4
        public int Pm_Lg { get; set; } = 4; //default 4
        public int Pm_Md { get; set; } = 6; //default 6
        public int Pm_Sm { get; set; } = 6; //default 6
        public int Pm_Col { get; set; } = 12; //default 12

        public int ManufacturerFeaturedProducts { get; set; } 
        public int MF_m_Xl { get; set; } = 4; //default 4
        public int MF_m_Lg { get; set; } = 4; //default 4
        public int MF_m_Md { get; set; } = 6; //default 6
        public int MF_m_Sm { get; set; } = 6; //default 6
        public int MF_m_Col { get; set; } = 6; //default 6
        public int MF_p_Xl { get; set; } = 8; //default 8
        public int MF_p_Lg { get; set; } = 8; //default 8
        public int MF_p_Md { get; set; } = 12; //default 12
        public int MF_p_Sm { get; set; } = 12; //default 12
        public int MF_p_Col { get; set; } = 12; //default 12
        public int MF_pp_Xl { get; set; } = 6; //default 6
        public int MF_pp_Lg { get; set; } = 6; //default 6
        public int MF_pp_Md { get; set; } = 6; //default 6
        public int MF_pp_Sm { get; set; } = 6; //default 6
        public int MF_pp_Col { get; set; } = 12; //default 12


        //Home page news
        public int HomePageNews { get; set; }
        public int Pn_Xl { get; set; } = 4; //default 4
        public int Pn_Lg { get; set; } = 4; //default 4
        public int Pn_Md { get; set; } = 6; //default 6
        public int Pn_Sm { get; set; } = 6; //default 6
        public int Pn_Col { get; set; } = 12; //default 12


        public int HomePageBlog { get; set; }
        public int Pb_Xl { get; set; } = 6; //default 6
        public int Pb_Lg { get; set; } = 6; //default 6 
        public int Pb_Md { get; set; } = 6; //default 6
        public int Pb_Sm { get; set; } = 6; //default 6
        public int Pb_Col { get; set; } = 12; //default 12

        public int HomePagePolls { get; set; } 
    }
}
