import usedByA from './used-by-a';

/**
 * You can import the previous instance of your module as you would any other module.
 * On first load, module == false.
 */
import { module } from '@hot';

/** 
 * Since all exports of the previous instance are available, you can simply export any state you might want to persist.
 *
 * Here we set and export the state of the file. If 'module == false' (first load),
 * then initialise the state to {}, otherwise set the state to the previously exported
 * state.
 */
export const _state = module ? module._state : {};

/**
 * If you're module needs to run some 'cleanup' code before being unloaded from the system, it can do so,
 * by exporting an `__unload` function that will be run just before the module is deleted from the registry.
 *
 * Here you would unsubscribe from listeners, or any other task that might cause issues in your application,
 * or prevent the module from being garbage collected.
 *
 * See SystemJS.unload API for more information.
 */
export const __unload = () => {
    console.log('Unload a a something (unsubscribe from listeners, disconnect from socket, etc...)')
    // force unload React components
   // ReactDOM.unmountComponentAtNode(DOMNode);	// your container node
}

import('./dynamically-imported/apply-color-and-message').then(({ default: apply }) => {
    apply('#a [data-used-by="a"]', usedByA);    
});