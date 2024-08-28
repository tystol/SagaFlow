import CommonHelper                     from "@/utils/CommonHelper";
import { replace }                      from "svelte-spa-router";
import { get }                          from "svelte/store";
import { addErrorToast }                from "@/stores/toasts";
import { setErrors }                    from "@/stores/errors";
import { protectedFilesCollectionsCache } from "@/stores/collections";
import sagaFlow from "../state/SagaFlowState";


// TODO: write a proper js SDK wrapper for SagaFlow API

class ResourceListService {
    constructor(id) {
        this.id = id;
    }

    getList(){
        return sagaFlow.getResources(this.id).then(resources =>{
            return {
                page: 1,
                perPage: 1000,
                totalItems: resources.length,
                totalPages: 1,
                items: resources
            };
        });
    }
}

class SagaFlowApiClient {

    resourceList(id){
        return new ResourceListService(id);
    }
    
    /**
     * Generic API error response handler.
     *
     * @param  {Error}   err        The API error itself.
     * @param  {Boolean} notify     Whether to add a toast notification.
     * @param  {String}  defaultMsg Default toast notification message if the error doesn't have one.
     */
    error(err, notify = true, defaultMsg = "") {
        if (!err || !(err instanceof Error) || err.isAbort) {
            return;
        }

        const statusCode = (err?.status << 0) || 400;
        const responseData = err?.data || {};
        const msg = responseData.message || err.message || defaultMsg;

        // add toast error notification
        if (notify && msg) {
            addErrorToast(msg);
        }

        // populate form field errors
        if (!CommonHelper.isEmpty(responseData.data)) {
            setErrors(responseData.data);
        }

        // unauthorized
        if (statusCode === 401) {
            this.cancelAllRequests();
            return this.logout();
        }

        // forbidden
        if (statusCode === 403) {
            this.cancelAllRequests();
            return replace("/");
        }
    };
}

const sagaFlowClient = new SagaFlowApiClient();

export default sagaFlowClient;
