class CatalogueScanner_ColesOnline {
    static instance = new CatalogueScanner_ColesOnline();

    get #productListControler() {
        return angular.element(document.querySelector('[data-colrs-product-list]')).data()?.$colrsProductListController;
    }

    get productListData() {
        return this.#productListControler?.widget?.data;
    }

    get isDataLoaded() {
        const productListData = this.productListData;
        return typeof productListData?.products[productListData?.products.length - 1].name === 'string'
    }

    get #paginationController() {
        return angular.element(document.querySelector('[data-colrs-pagination]')).data()?.$colrsPaginationController;
    }

    get isPaginationLoaded() {
        return typeof this.#paginationController !== 'undefined';
    }

    get currentPageNum() {
        return this.#paginationController.currPageNum;
    }

    get totalPageCount() {
        return this.#paginationController.totalPageCount;
    }

    async nextPage() {
        return await this.#productListControler.pagination(this.currentPageNum + 1);
    }
}
