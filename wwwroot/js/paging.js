/**
 * API: GetChapterAPI(string storySlug, int pageNumber = 1)
 * Return: {
        data: [...] 
        totalPages 
        currentPage
 * }
 * Usage: 
 * var url = 'API to get data'
 * var formData = {
 *      abc: xyz,
 * }
 * Paging.CreatePaging("#paging", ".list-chapter ul", url, formData, (response) => {
 *      return Your html with data from api
 * })
 */
class Paging {
    static CreatePaging(pagingContainer, dataContainer, url, formData, callback) {
        this.pagingContainer = pagingContainer;
        this.dataContainer = dataContainer;
        this.callback = callback;
        this.url = url;
        this.formData = formData;

        this.GetPageData();
    }
    static PagingTemplate = (totalPage, currentPage) => {
        var template = "";

        var FirstPage = 1;
        var LastPage = totalPage;

        if (currentPage > totalPage) currentPage = totalPage;

        var ForwardPage = null;
        if (currentPage < totalPage) ForwardPage = currentPage + 1;

        var BackwardPage = null;
        if (currentPage > 1) BackwardPage = currentPage - 1;

        var PageNumberArray = Array();

        var delta = 3; // Số trang mở rộng về mỗi bên trang hiện tại
        var remain = delta * 2; // Số trang hai bên trang hiện tại
        PageNumberArray.push(currentPage);
        // Các trang phát triển về hai bên trang hiện tại
        for (let i = 1; i <= delta; i++) {
            if (currentPage + i <= totalPage) {
                PageNumberArray.push(currentPage + i);
                remain--;
            }

            if (currentPage - i >= 1) {
                PageNumberArray = [currentPage - i, ...PageNumberArray];
                remain--;
            }
        }
        if (remain > 0) {
            if (PageNumberArray[0] == 1) {
                for (let i = 1; i <= remain; i++) {
                    if (
                        PageNumberArray[PageNumberArray.length - 1] + 1 <=
                        totalPage
                    ) {
                        PageNumberArray.push(
                            PageNumberArray[PageNumberArray.length - 1] + 1
                        );
                    }
                }
            } else {
                for (let i = 1; i <= remain; i++) {
                    if (PageNumberArray[0] - 1 > 1) {
                        PageNumberArray = [
                            PageNumberArray[0] - 1,
                            ...PageNumberArray,
                        ];
                    }
                }
            }
        }

        template += `<ul class="pagination justify-content-center">`;
        if (currentPage != 1)
            template += `<li class="page-item">
                            <a class="page-link" data-page=${FirstPage}>Trang đầu</a>
                        </li>`;

        if (BackwardPage != null) {
            template += `<li class="page-item">
                            <a class="page-link"  data-page=${BackwardPage}><</a>
                        </li>`;
        }

        var numberingLoop = "";
        PageNumberArray.forEach((page) => {
            let isActive = "";
            let event = "";
            if (page == currentPage) {
                isActive = "active";
            } else {
                event = `data-page=${page}`;
            }

            numberingLoop += `<li class="page-item ${isActive}">
                                <a class="page-link" ${event} tabindex="-1">${page}</a>
                            </li>`;
        });
        template += numberingLoop;

        if (ForwardPage != null)
            template += `<li class="page-item">
                            <a class="page-link" data-page=${ForwardPage}>></a>
                        </li>`;

        if (currentPage != totalPage)
            template += `<li class="page-item">
                            <a class="page-link" data-page=${LastPage}>Trang cuối</a>
                        </li>`;

        template += `</ul>`;
        $(this.pagingContainer).append(template);
        var _this = this;
        $(".page-link").click(function(e)  {
            e.preventDefault();
            var page = $(this).data("page");
            if (page != null) 
                _this.GetPageData(page);
        })
    };
    static GetPageData = (pageNumber = 1) => {
        if (pageNumber == null) return false;
        $(this.pagingContainer).empty();
        $(this.dataContainer).empty();

        $.ajax({
            url: this.url,
            data: {
                ...this.formData,
                pageNumber: pageNumber
            },
            success: (res) => {
                if (res.totalPages === 0) return;
                var html = this.callback(res);
                
                $(this.dataContainer).append(html).hide().fadeIn();
                this.PagingTemplate(res.totalPages, res.currentPage);
            },
        });
    };
}
