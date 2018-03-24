using System;
using System.Collections.Generic;
using System.Linq;

namespace app_el_sys
{

    public class EL
    {
        public const int _SPEAK_RATE_DEFAULT_WORD = -5; // -10 -> 10
        public const int _SPEAK_RATE_DEFAULT_CLAUSE = -5; // -10 -> 10
        public const int _SPEAK_RATE_DEFAULT_SENTENCE = -5; // -10 -> 10
        public const int _SPEAK_RATE_DEFAULT_PARAGRAPH = -5; // -10 -> 10

        public const int _TIMEOUT_SPEAK_WORD = 0;
        public const int _TIMEOUT_SPEAK_CLAUSE = 1000;
        public const int _TIMEOUT_SPEAK_SENTENCE = 3000;

        public const string _SOCKET_CMD_REPLAY = "REPLAY";
        public const string _SOCKET_CMD_STOP = "STOP";
        public const string _SOCKET_CMD_TRANSLATE = "TRANSLATE";

        public const string _STATUS_SPEAK_OK = "OK";
        public const string _STATUS_SPEAK_FAIL = "FAIL";

        public static readonly string[] _WORD_SKIP_WHEN_READING = { "is", "are", "was", "were", "the", "to", "and", "of" };

        public static readonly string[] _SPLIT_PARAGRAPH_TO_SENTENCE = { "." };
        public static readonly string[] _SPLIT_PARAGRAPH_TO_CLAUSE = { ":", ",", "(", ")", "when", "that", "from", "of" };

        public const string _TAG_CODE_CHAR_BEGIN = "//#";
        public const string _TAG_CODE_CHAR_END = "//.";

        public const string DO_SPEECH_ALL = "__s_0";
        public const string DO_SPEECH_WORD = "__s_1";
        public const string DO_SPEECH_PARAGRAPH = "__s_2";
        public const string DO_SPEECH_SENTENCE = "__s_3";
        public const string DO_SPEECH_SENTENCE_KEY_WORD = "__s_4";

        public const string TAG_CODE = "pre";
        public const string TAG_ARTICLE = "article";
        public const string TAG_TITLE = "h2";
        public const string TAG_HEADING = "h3";
        public const string TAG_LINK = "a";
        public const string TAG_NOTE = "span";
        public const string TAG_PARAGRAPH = "p";
        public const string TAG_SENTENCE = "b";
        public const string TAG_CLAUSE = "em";
        public const string TAG_WORD = "i";

        public const string ATTR_SPEECH_WORD_VOCABULARY = "nv";       // word is vocabulary new: tu moi
        public const string ATTR_SPEECH_WORD_QUESTION_WH = "wh";       // what, where, which, how: many, much ...
        public const string ATTR_SPEECH_WORD_VERB_TOBE = "vb";       // word is verb tobe: is, am, are, be, will, was, were
        public const string ATTR_SPEECH_WORD_VERB_MODAL = "vm";       // word is modal verb: Động từ khuyết thiếu
        public const string ATTR_SPEECH_WORD_VERB_INFINITIVE = "vi";       // word is verb infinitive        
        public const string ATTR_SPEECH_WORD_IDOM = "id";       // word is idom or struct grammar   
        public const string ATTR_SPEECH_WORD_GRAMMAR = "gm";       // word is idom or struct grammar
        public const string ATTR_SPEECH_WORD_SPECIALIZE = "sp";       // word is specialize

        /*
        WHO   (ai)
        WHOM  (ai)
        WHOSE (của ai) : chỉ sở hữu 
        WHAT  (gì, cái gỉ) : chỉ đồ vật, sự việc, hay con vật.
        WHICH (nào, cái nào): chỉ sự chọn lựa; chỉ đồ vật, sự việc hay con vật. 
        WHEN  (khi nào) : chỉ thời gian - 
        WHERE (đâu, ở đâu) : chỉ nơi chốn - 
        WHY   (tại sao) : chỉ lí do hoặc nguyên nhân. - 
        HOW   (thể nào, cách nào) : chỉ trạng thái, phương tiện hay phương pháp.
          */
        public const string WORDS_QUESTION_WH = "who,whom,whose,what,which,when,where,why,how";
        public const string WORDS_VERB_TOBE = "am,is,are,'m,'s,'re,amn't,isn't,aren't,was,were,wasn't,weren't";
        public const string WORDS_VERB_MODAL = "can,can’t,cannot,could,must,mustn’t,have to,may,might,will,would,shall,should,ought to,dare,need,needn’t,used to";
        public const string WORDS_VERB_INFINITIVE = "";
        public const string WORDS_SPECIALIZE = "";

        public readonly Dictionary<string, string> DEFINE = new Dictionary<string, string>() {
            { "bare-infinitive","động từ nguyên thể không “to” "}
        };

        public readonly Dictionary<string, string> GRAMMAR = new Dictionary<string, string>() {
            { "ought not to have","ought not to have + Vpp: diễn tả một sự không tán đồng về một hành động đã làm trong quá khứ, không phải làm điều gì đó"}
            ,{ "able to","was/were able to: Nếu câu nói hàm ý một sự thành công trong việc thực hiện hành động (succeeded in doing)" }

        };
        public readonly Dictionary<string, string> IDOM = new Dictionary<string, string>() { };

        #region [ WORDS_VERBS_IRREGULAR ]

        public const string WORDS_VERBS_IRREGULAR =
                                                    " abide ; abode , abided ; abode , abided ; " +                    //-"lưu trú ,  lưu lại"
                                                    " arise ; arose ; arisen ; " +                                     //-phát sinh
                                                    " awake ; awoke ; awoken ; " +                                     //-"đánh thức ,  thức"
                                                    " be ; was , were ; been ; " +                                     //-"thì ,  là ,  bị ,  ở"
                                                    " bear ; bore ; borne ; " +                                        //-"mang ,  chịu đựng"
                                                    " become ; became ; become ; " +                                   //-trở nên
                                                    " befall ; befell ; befallen ; " +                                 //-xảy đến
                                                    " begin ; began ; begun ; " +                                      //-bắt đầu
                                                    " behold ; beheld ; beheld ; " +                                   //-ngắm nhìn
                                                    " bend ; bent ; bent ; " +                                         //-bẻ cong
                                                    " beset ; beset ; beset ; " +                                      //-bao quanh
                                                    " bespeak ; bespoke ; bespoken ; " +                               //-chứng tỏ
                                                    " bid ; bid ; bid ; " +                                            //-trả giá
                                                    " bind ; bound ; bound ; " +                                       //-"buộc ,  trói"
                                                    " bleed ; bled ; bled ; " +                                        //-chảy máu
                                                    " blow ; blew ; blown ; " +                                        //-thổi
                                                    " break ; broke ; broken ; " +                                     //-đập vỡ
                                                    " breed ; bred ; bred ; " +                                        //-"nuôi ,  dạy dỗ"
                                                    " bring ; brought ; brought ; " +                                  //-mang đến
                                                    " broadcast ; broadcast ; broadcast ; " +                          //-phát thanh
                                                    " build ; built ; built ; " +                                      //-xây dựng
                                                    " burn ; burnt , burned ; burnt , burned ; " +                     //-"đốt ,  cháy"
                                                    " buy ; bought ; bought ; " +                                      //-mua
                                                    " cast ; cast ; cast ; " +                                         //-"ném ,  tung"
                                                    " catch ; caught ; caught ; " +                                    //-"bắt ,  chụp"
                                                    " chide ; chid , chided ; chid , chidden , chided ; " +            //-"mắng ,  chửi"
                                                    " choose ; chose ; chosen ; " +                                    //-"chọn ,  lựa"
                                                    " cleave ; clove , cleaved ; cloven , cleaved ; " +                //-"chẻ ,  tách hai"
                                                    " cleave ; clave ; cleaved ; " +                                   //-dính chặt
                                                    " come ; came ; come ; " +                                         //-"đến ,  đi đến"
                                                    " cost ; cost ; cost ; " +                                         //-có giá là
                                                    " crow ; crew , crewed ; crowed ; " +                              //-gáy (gà)
                                                    " cut ; cut ; cut ; " +                                            //-"cắn ,  chặt"
                                                    " deal ; dealt ; dealt ; " +                                       //-giao thiệp
                                                    " dig ; dug ; dug ; " +                                            //-dào
                                                    " dive ; dove , dived ; dived ; " +                                //-"lặn ,  lao xuống"
                                                    " draw ; drew ; drawn ; " +                                        //-"vẽ ,  kéo"
                                                    " dream ; dreamt , dreamed ; dreamt , dreamed ; " +                //-mơ thấy
                                                    " drink ; drank ; drunk ; " +                                      //-uống
                                                    " drive ; drove ; driven ; " +                                     //-lái xe
                                                    " dwell ; dwelt ; dwelt ; " +                                      //-"trú ngụ ,  ở"
                                                    " eat ; ate ; eaten ; " +                                          //-ăn
                                                    " fall ; fell ; fallen ; " +                                       //-"ngã ,  rơi"
                                                    " feed ; fed ; fed ; " +                                           //-"cho ăn ,  ăn ,  nuôi"
                                                    " feel ; felt ; felt ; " +                                         //-cảm thấy
                                                    " fight ; fought ; fought ; " +                                    //-chiến đấu
                                                    " find ; found ; found ; " +                                       //-"tìm thấy ,  thấy"
                                                    " flee ; fled ; fled ; " +                                         //-chạy trốn
                                                    " fling ; flung ; flung ; " +                                      //-tung ;  quang
                                                    " fly ; flew ; flown ; " +                                         //-bay
                                                    " forbear ; forbore ; forborne ; " +                               //-nhịn
                                                    " forbid ; forbade , forbad ; forbidden ; " +                      //-"cấm ,  cấm đoán"
                                                    " forecast ; forecast , forecasted ; forecast , forecasted ; " +   //-tiên đoán
                                                    " foresee ; foresaw ; forseen ; " +                                //-thấy trước
                                                    " foretell ; foretold ; foretold ; " +                             //-đoán trước
                                                    " forget ; forgot ; forgotten ; " +                                //-quên
                                                    " forgive ; forgave ; forgiven ; " +                               //-tha thứ
                                                    " forsake ; forsook ; forsaken ; " +                               //-ruồng bỏ
                                                    " freeze ; froze ; frozen ; " +                                    //-(làm) đông lại
                                                    " get ; got ; got , gotten ; " +                                   //-có được
                                                    " gild ; gilt , gilded ; gilt , gilded ; " +                       //-mạ vàng
                                                    " gird ; girt , girded ; girt , girded ; " +                       //-đeo vào
                                                    " give ; gave ; given ; " +                                        //-cho
                                                    " go ; went ; gone ; " +                                           //-đi
                                                    " grind ; ground ; ground ; " +                                    //-"nghiền ,  xay"
                                                    " grow ; grew ; grown ; " +                                        //-"mọc ,  trồng"
                                                    " hang ; hung ; hung ; " +                                         //-"móc lên ,  treo lên"
                                                    " hear ; heard ; heard ; " +                                       //-nghe
                                                    " heave ; hove , heaved ; hove , heaved ; " +                      //-trục lên
                                                    " hide ; hid ; hidden ; " +                                        //-"giấu ,  trốn ,  nấp"
                                                    " hit ; hit ; hit ; " +                                            //-đụng
                                                    " hurt ; hurt ; hurt ; " +                                         //-làm đau
                                                    " inlay ; inlaid ; inlaid ; " +                                    //-"cẩn ,  khảm"
                                                    " input ; input ; input ; " +                                      //-đưa vào (máy điện toán)
                                                    " inset ; inset ; inset ; " +                                      //-"dát ,  ghép"
                                                    " keep ; kept ; kept ; " +                                         //-giữ
                                                    " kneel ; knelt , kneeled ; knelt , kneeled ; " +                  //-quỳ
                                                    " knit ; knit , knitted ; knit , knitted ; " +                     //-đan
                                                    " know ; knew ; known ; " +                                        //-"biết ,  quen biết"
                                                    " lay ; laid ; laid ; " +                                          //-"đặt ,  để"
                                                    " lead ; led ; led ; " +                                           //-"dẫn dắt ,  lãnh đạo"
                                                    " leap ; leapt ; leapt ; " +                                       //-"nhảy ,  nhảy qua"
                                                    " learn ; learnt , learned ; learnt , learned ; " +                //-"học ,  được biết"
                                                    " leave ; left ; left ; " +                                        //-"ra đi ,  để lại"
                                                    " lend ; lent ; lent ; " +                                         //-cho mượn (vay)
                                                    " let ; let ; let ; " +                                            //-"cho phép ,  để cho"
                                                    " lie ; lay ; lain ; " +                                           //-nằm
                                                    " light ; lit , lighted ; lit ,  lighted ; " +                     //-thắp sáng
                                                    " lose ; lost ; lost ; " +                                         //-"làm mất ,  mất"
                                                    " make ; made ; made ; " +                                         //-"chế tạo ,  sản xuất"
                                                    " mean ; meant ; meant ; " +                                       //-có nghĩa là
                                                    " meet ; met ; met ; " +                                           //-gặp mặt
                                                    " mislay ; mislaid ; mislaid ; " +                                 //-để lạc mất
                                                    " misread ; misread ; misread ; " +                                //-đọc sai
                                                    " misspell ; misspelt ; misspelt ; " +                             //-viết sai chính tả
                                                    " mistake ; mistook ; mistaken ; " +                               //-"phạm lỗi ,  lầm lẫn"
                                                    " misunderstand ; misunderstood ; misunderstood ; " +              //-hiểu lầm
                                                    " mow ; mowed ; mown , mowed ; " +                                 //-cắt cỏ
                                                    " outbid ; outbid ; outbid ; " +                                   //-trả hơn giá
                                                    " outdo ; outdid ; outdone ; " +                                   //-làm giỏi hơn
                                                    " outgrow ; outgrew ; outgrown ; " +                               //-lớn nhanh hơn
                                                    " output ; output ; output ; " +                                   //-cho ra (dữ kiện)
                                                    " outrun ; outran ; outrun ; " +                                   //-chạy nhanh hơn ;  vượt giá
                                                    " outsell ; outsold ; outsold ; " +                                //-bán nhanh hơn
                                                    " overcome ; overcame ; overcome ; " +                             //-khắc phục
                                                    " overeat ; overate ; overeaten ; " +                              //-ăn quá nhiều
                                                    " overfly ; overflew ; overflown ; " +                             //-bay qua
                                                    " overhang ; overhung ; overhung ; " +                             //-"nhô lên trên ,  treo lơ lửng"
                                                    " overhear ; overheard ; overheard ; " +                           //-nghe trộm
                                                    " overlay ; overlaid ; overlaid ; " +                              //-phủ lên
                                                    " overpay ; overpaid ; overpaid ; " +                              //-trả quá tiền
                                                    " overrun ; overran ; overrun ; " +                                //-tràn ngập
                                                    " oversee ; oversaw ; overseen ; " +                               //-trông nom
                                                    " overshoot ; overshot ; overshot ; " +                            //-đi quá đích
                                                    " oversleep ; overslept ; overslept ; " +                          //-ngủ quên
                                                    " overtake ; overtook ; overtaken ; " +                            //-đuổi bắt kịp
                                                    " overthrow ; overthrew ; overthrown ; " +                         //-lật đổ
                                                    " pay ; paid ; paid ; " +                                          //-trả (tiền)
                                                    " prove ; proved ; proven , proved ; " +                           //-chứng minh (tỏ)
                                                    " put ; put ; put ; đặt ;  " +                                     //-để
                                                    " read ; read ; read ; " +                                         //-đọc
                                                    " rebuild ; rebuilt ; rebuilt ; " +                                //-xây dựng lại
                                                    " redo ; redid ; redone ; " +                                      //-làm lại
                                                    " remake ; remade ; remade ; " +                                   //-làm lại ; chế tạo lại
                                                    " rend ; rent ; rent ; " +                                         //-toạc ra ;  xé
                                                    " repay ; repaid ; repaid ; " +                                    //-hoàn tiền lại
                                                    " resell ; resold ; resold ; " +                                   //-bán lại
                                                    " retake ; retook ; retaken ; " +                                  //-chiếm lại ;  tái chiếm
                                                    " rewrite ; rewrote ; rewritten ; " +                              //-viết lại
                                                    " rid ; rid ; rid ; " +                                            //-giải thoát
                                                    " ride ; rode ; ridden ; " +                                       //-cưỡi
                                                    " ring ; rang ; rung ; " +                                         //-rung chuông
                                                    " rise ; rose ; risen ; " +                                        //-đứng dậy ;  mọc
                                                    " run ; ran ; run ; " +                                            //-chạy
                                                    " saw ; sawed ; sawn ; " +                                         //-cưa
                                                    " say ; said ; said ; " +                                          //-nói
                                                    " see ; saw ; seen ; " +                                           //-nhìn thấy
                                                    " seek ; sought ; sought ; " +                                     //-tìm kiếm
                                                    " sell ; sold ; sold ; " +                                         //-bán
                                                    " send ; sent ; sent ; " +                                         //-gửi
                                                    " sew ; sewed ; sewn , sewed ; " +                                 //-may
                                                    " shake ; shook ; shaken ; " +                                     //-lay ;  lắc
                                                    " shear ; sheared ; shorn ; " +                                    //-xén lông (Cừu)
                                                    " shed ; shed ; shed ; rơi ;  " +                                  //-rụng
                                                    " shine ; shone ; shone ; " +                                      //-chiếu sáng
                                                    " shoot ; shot ; shot ; " +                                        //-bắn
                                                    " show ; showed ; shown , showed ; " +                             //-cho xem
                                                    " shrink ; shrank ; shrunk ; " +                                   //-co rút
                                                    " shut ; shut ; shut ; " +                                         //-đóng lại
                                                    " sing ; sang ; sung ; " +                                         //-ca hát
                                                    " sink ; sank ; sunk ; " +                                         //-chìm ;  lặn
                                                    " sit ; sat ; sat ; " +                                            //-ngồi
                                                    " slay ; slew ; slain ; " +                                        //-sát hại ;  giết hại
                                                    " sleep ; slept ; slept ; " +                                      //-ngủ
                                                    " slide ; slid ; slid ; " +                                        //-trượt ;  lướt
                                                    " sling ; slung ; slung ; " +                                      //-ném mạnh
                                                    " slink ; slunk ; slunk ; " +                                      //-lẻn đi
                                                    " smell ; smelt ; smelt ; " +                                      //-ngửi
                                                    " smite ; smote ; smitten ; " +                                    //-đập mạnh
                                                    " sow ; sowed ; sown , sewed ; " +                                 //-gieo ;  rải
                                                    " speak ; spoke ; spoken ; " +                                     //-nói
                                                    " speed ; sped , speeded ; sped , speeded ; " +                    //-chạy vụt
                                                    " spell ; spelt , spelled ; spelt , spelled ; " +                  //-đánh vần
                                                    " spend ; spent ; spent ; " +                                      //-tiêu sài
                                                    " spill ; spilt , spilled ; spilt , spilled ; " +                  //-tràn ;  đổ ra
                                                    " spin ; spun , span ; spun ; " +                                  //-quay sợi
                                                    " spit ; spat ; spat ; " +                                         //-khạc nhổ
                                                    " spoil ; spoilt , spoiled ; spoilt , spoiled ; " +                //-làm hỏng
                                                    " spread ; spread ; spread ; " +                                   //-lan truyền
                                                    " spring ; sprang ; sprung ; " +                                   //-nhảy
                                                    " stand ; stood ; stood ; " +                                      //-đứng
                                                    " stave ; stove , staved ; stove , staved ; " +                    //-đâm thủng
                                                    " steal ; stole ; stolen ; " +                                     //-đánh cắp
                                                    " stick ; stuck ; stuck ; " +                                      //-ghim vào ;  đính
                                                    " sting ; stung ; stung ; " +                                      //-châm  ;  chích ;  đốt
                                                    " stink ; stunk , stank ; stunk ; " +                              //-bốc mùi hôi
                                                    " strew ; strewed ; strewn , strewed ; " +                         //-"rắc  ,  rải"
                                                    " stride ; strode ; stridden ; " +                                 //-bước sải
                                                    " strike ; struck ; struck ; " +                                   //-đánh đập
                                                    " string ; strung ; strung ; " +                                   //-gắn dây vào
                                                    " strive ; strove ; striven ; " +                                  //-cố sức
                                                    " swear ; swore ; sworn ; " +                                      //-tuyên thệ
                                                    " sweep ; swept ; swept ; " +                                      //-quét
                                                    " swell ; swelled ; swollen , swelled ; " +                        //-phồng ;  sưng
                                                    " swim ; swam ; swum ; " +                                         //-bơi lội
                                                    " swing ; swung ; swung ; " +                                      //-đong đưa
                                                    " take ; took ; taken ; " +                                        //-cầm ; lấy
                                                    " teach ; taught ; taught ; " +                                    //-dạy ; giảng dạy
                                                    " tear ; tore ; torn ; " +                                         //-xé ; rách
                                                    " tell ; told ; told ; " +                                         //-kể ; bảo
                                                    " think ; thought ; thought ; " +                                  //-suy nghĩ
                                                    " throw ; threw ; thrown ; " +                                     //-ném  ;  liệng
                                                    " thrust ; thrust ; thrust ; " +                                   //-thọc  ; nhấn
                                                    " tread ; trod ; trodden , trod ; " +                              //-giẫm  ;  đạp
                                                    " unbend ; unbent ; unbent ; " +                                   //-làm thẳng lại
                                                    " undercut ; undercut ; undercut ; " +                             //-ra giá rẻ hơn
                                                    " undergo ; underwent ; undergone ; " +                            //-kinh qua
                                                    " underlie ; underlay ; underlain ; " +                            //-nằm dưới
                                                    " underpay ; underpaid ; underpaid ; " +                           //-trả lương thấp
                                                    " undersell ; undersold ; undersold ; " +                          //-bán rẻ hơn
                                                    " understand ; understood ; understood ; " +                       //-hiểu
                                                    " undertake ; undertook ; undertaken ; " +                         //-đảm nhận
                                                    " underwrite ; underwrote ; underwritten ; " +                     //-bảo hiểm
                                                    " undo ; undid ; undone ; " +                                      //-tháo ra
                                                    " unfreeze ; unfroze ; unfrozen ; " +                              //-làm tan đông
                                                    " unwind ; unwound ; unwound ; " +                                 //-tháo ra
                                                    " uphold ; upheld ; upheld ; " +                                   //-ủng hộ
                                                    " upset ; upset ; upset ; " +                                      //-đánh đổ ;  lật đổ
                                                    " wake ; woke , waked ; woken , waked ; " +                        //-thức giấc
                                                    " waylay ; waylaid ; waylaid ; " +                                 //-mai phục
                                                    " wear ; wore ; worn ; " +                                         //-mặc
                                                    " weave ; wove , weaved ; woven , weaved ; " +                     //-dệt
                                                    " wed ; wed , wedded ; wed , wedded ; " +                          //-kết hôn
                                                    " weep ; wept ; wept ; " +                                         //-khóc
                                                    " wet ; wet , wetted ; wet , wetted ; " +                          //-làm ướt
                                                    " win ; won ; won ; thắng  ;  " +                                  //-chiến thắng
                                                    " wind ; wound ; wound ; " +                                       //-quấn
                                                    " withdraw ; withdrew ; withdrawn ; " +                            //-rút lui
                                                    " withhold ; withheld ; withheld ; " +                             //-từ khước
                                                    " withstand ; withstood ; withstood ; " +                          //-cầm cự
                                                    " work ; worked ;  worked ; " +                                    //-"rèn (sắt) ,  nhào nặng đất"
                                                    " wring ; wrung ; wrung ; " +                                      //-vặn  ;  siết chặt
                                                    " write ; wrote ; written ; ";                                    //-viết
        #endregion
        
    }

    /// <summary>
    /// speech type
    /// </summary>
    public enum EL_SPEAK_TYPE
    {
        /// <summary>
        /// chỉ đọc các từ chính
        /// </summary>
        SPEAK_WORD,
        /// <summary>
        /// chỉ đọc các từ chính
        /// </summary>
        SPEAK_KEYWORD,
        /// <summary>
        /// đọc cụm từ trong câu
        /// </summary>
        SPEAK_CLAUSE,
        /// <summary>
        /// đọc cả câu cách nhau bởi dấu (.)
        /// </summary>
        SPEAK_SENTENCE,
        /// <summary>
        /// đọc cả 1 đoạn gồm nhiều câu
        /// </summary>
        SPEAK_PARAGRAPH,
    }

}
