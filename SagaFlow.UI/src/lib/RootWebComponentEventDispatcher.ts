import {createEventDispatcher, getContext, setContext} from "svelte";

// Added the prop __webComponentElement to the svelte component that will contain Html Element that is the root of the web component,
// This is used to dispatch custom events for the web component.
export const extendToGetWebComponentRoot =  (ctor) => class extends ctor {
    constructor() {
        super();

        this.__webComponentElement = this;
    }
}

// 
export class RootWebComponentEventDispatcher {
    constructor(private rootElement: HTMLElement) {
    }
    
    public dispatch<TDetail>(eventName: string, detail: TDetail)
    {
        this.rootElement.dispatchEvent(
            new CustomEvent<TDetail>(
                eventName,
                {
                    detail,
                    bubbles: true,
                    composed: true,
                }
            )
        )
    }
}

export const RootWebComponentEventDispatcherContextKey: string = "RootWebComponentEventDispatcher";

// We need to dispatch all web component events from the Web Component's root html element regardless of whether the Svelte component is the 
// web component or simply a svelte component withing a larger svelte component.  If the Svelte component is the root component in Web Component
// it will set it the RootWebComponentEventDispatcherContextKey context with its RootWebComponentEventDispatcher, and child svelte component
// will look for that context when dispatching events for the WebComponent.
// eg. The CommentForm component can exist as a Web Component on its own when used in an application as sf-command-form, it will dispatch events
// from the HTML element sf-command-form.  However when used as part of the Main which is used as the web component sf-main, then a command event
// is dispatch, it needs to be dispatched from the html element sf-main.
// We determine if a Svelte component is the root component in a Web Component if it has a root web component html element, if is doesn't have one
// it is being used as a child svelte component in a larger svelte component.
export const setupRootWebComponentEventDispatcherContext = (rootWebComponentHtmlElement: HTMLElement | undefined): RootWebComponentEventDispatcher | undefined => {

    // If the svelte component isn't being used as a web component, ie. could be part of a larger svelte component that is being expose as a web component,
    // or simply not part of the web component at all, then simple look for a parent context and return it.
    if (!rootWebComponentHtmlElement) {
        // The current svelte component isn't the root svelte component of a web component, so try to fetch one from the current context,
        // there might not be one in-case we are using the default saga-flow SPA exposed on the SagaFlow route.
        return getContext<RootWebComponentEventDispatcher>(RootWebComponentEventDispatcherContextKey);
    }
    
    const rootWebComponentEventDispatcher = new RootWebComponentEventDispatcher(rootWebComponentHtmlElement);
    setContext(RootWebComponentEventDispatcherContextKey, rootWebComponentEventDispatcher);
    
   return rootWebComponentEventDispatcher;
}

type EventDispatcher = <T>(eventName:string, detail: T) => void;

// Creates a higher order function that will use the resolved rootWebComponentEventDispatcher of the root svelte component 
// used to compose a web component.  It will also resolve a standard svelte event dispatcher for vanilla svelte component
// events.
export const createWebComponentEventDispatcher =  (rootWebComponentHtmlElement: HTMLElement | undefined): EventDispatcher => {
    const rootWebComponentEventDispatcher: RootWebComponentEventDispatcher | undefined = setupRootWebComponentEventDispatcherContext(rootWebComponentHtmlElement);
    const svelteEventDispatcher = createEventDispatcher();
    
    return <T>(eventName: string, detail:T) => {
        // Need to handle null in-case we are on the SagaFlow route, where no WebComponents are being used.
        rootWebComponentEventDispatcher?.dispatch(eventName, detail);

        svelteEventDispatcher(eventName, detail);
    };
}