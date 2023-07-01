import cv2
import os
from tensorflow.keras.models import load_model
import numpy as np
from IPython.display import Image,display
import matplotlib.pyplot as plt
from ultralytics import YOLO
from enum import Enum


class BodyPart(Enum):
    ELBOW = 0
    FINGER = 1
    FOREARM = 2
    HAND = 3
    HUMERUS = 4

def main():
    binary_result_value = ''
    class_result_value = ''
    number_of_boxes = 0
    added_sentence = ''
    img_size=(350,350)

    img = cv2.imread(os.path.join('D://FYP//image_to_predict//image1.png'))  # pre-processed directory
    height, width, _ = img.shape
    img = cv2.resize(img, img_size)
    img=cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)

    model_img = np.expand_dims(img, axis=0)
    model_img = np.expand_dims(model_img, axis=-1)
    model_img = np.repeat(model_img, 3, axis=-1)    

    #load binary model
    binary_model = load_model('D://FYP//models//model_vgg16_binary.h5',compile=False)
    binary_result=binary_model.predict(model_img)

    if binary_result >= 0.5:
        binary_result_value='positive'
    else:
        binary_result_value='negative'
    
    added_sentence= 'Binary result value : '+binary_result_value+'\n'

    print('--------------------------------------------------------------------')
    
    with open('D://FYP//prediction_result.txt', 'w') as file:
            file.write(added_sentence)
    print('Binary result value : ',binary_result_value)
    
    print('--------------------------------------------------------------------')

    #check if positive and load multi class and yolo model

    if binary_result_value == 'positive':

        #load multiclass model

        multi_class_model = load_model('D://FYP/models//model_vgg16_multiclass.h5',compile=False)
        multi_class_result=multi_class_model.predict(model_img)
        predicted_class_index = np.argmax(multi_class_result)
            
        predicted_body_part = BodyPart(predicted_class_index)
        class_result_value = predicted_body_part.name.lower()
        added_sentence= 'Class result value : '+class_result_value+'\n'
        
        print('--------------------------------------------------------------------')
        
        with open('D://FYP//prediction_result.txt', 'a') as file:
            file.write(added_sentence)
            
        print('Class result value:',class_result_value)
        
        print('--------------------------------------------------------------------')

        #load YOLOv8 model
        yolo_model = YOLO("D://FYP//models//model_yolo.pt")  # load a custom model
        yolo_results = yolo_model("D://FYP//image_to_predict//image1.png",save=True)  # predict on an image

        for result in yolo_results:
            boxes = result.boxes  # Boxes object for bbox outputs    

        number_of_boxes=len(boxes)
        added_sentence= 'Number of boxes : '+str(number_of_boxes)+'\n'

        print('--------------------------------------------------------------------')
        
        with open('D://FYP//prediction_result.txt', 'a') as file:
            file.write(added_sentence)
            
        print('Number of boxes:',number_of_boxes)
        
        print('--------------------------------------------------------------------')

        return(number_of_boxes)
    
if __name__ == "__main__":
    main()