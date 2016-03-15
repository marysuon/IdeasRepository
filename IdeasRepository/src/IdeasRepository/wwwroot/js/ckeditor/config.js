/*
Copyright (c) 2003-2013, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.editorConfig = function( config )
{
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
    // config.uiColor = '#AADC6E';
    config.toolbar = 'Basic';
    //config.uiColor = "#262626";
    config.skin = 'kama';
};
CKEDITOR.on('instanceReady', function(e) {
    $('.cke_top').css('background', '#333');
    $('.cke_wrapper').css('background', '#333');
    $('span.cke_skin_kama').css('padding', '0');
    $('span.cke_skin_kama').css('border', '#333');
});
//{-moz-border-radius:5px;-webkit-border-radius:5px;border-radius:5px;border:1px solid #D3D3D3;padding:5px;}
